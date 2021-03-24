using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;

namespace PluginJira.API.Utility.EndpointHelperEndpoints
{
    public static class SourcesEndpointHelper
    {
        private class SourcesResponse
        {
            public List<Dictionary<string, object>>? Sources { get; set; }
        }

        private class SourcesEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                var response = await apiClient.GetAsync(
                    $"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

                var recordsList =
                    JsonConvert.DeserializeObject<SourcesResponse>(await response.Content.ReadAsStringAsync());

                if (recordsList.Sources == null)
                {
                    yield break;
                }
                
                foreach (var recordMap in recordsList.Sources)
                {
                    var normalizedRecordMap = new Dictionary<string, object?>();

                    foreach (var kv in recordMap)
                    {
                        if (
                            !string.IsNullOrWhiteSpace(DetailPath) &&
                            !string.IsNullOrWhiteSpace(DetailPropertyId) &&
                            kv.Key.Equals(DetailPropertyId) && kv.Value != null)
                        {
                            var detailResponse =
                                await apiClient.GetAsync(
                                    $"{BasePath.TrimEnd('/')}/{DetailPath.TrimStart('/')}/{kv.Value}");

                            var detailsRecord =
                                JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                    await detailResponse.Content.ReadAsStringAsync());

                            foreach (var detailKv in detailsRecord)
                            {
                                if (detailKv.Key.Equals(EndpointHelper.LinksPropertyId))
                                {
                                    continue;
                                }

                                normalizedRecordMap.TryAdd(detailKv.Key, detailKv.Value);
                            }

                            continue;
                        }

                        if (kv.Key.Equals(EndpointHelper.LinksPropertyId))
                        {
                            continue;
                        }

                        normalizedRecordMap.TryAdd(kv.Key, kv.Value);
                    }

                    yield return new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(normalizedRecordMap)
                    };
                }
            }
        }

        public static readonly Dictionary<string, Endpoint> SourcesEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllSources", new SourcesEndpoint
                {
                    Id = "AllSources",
                    Name = "All Sources",
                    BasePath = "/Sources",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "SourceID",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "SourceID"
                    }
                }
            },
        };
    }
}