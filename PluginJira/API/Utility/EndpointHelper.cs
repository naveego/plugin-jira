using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginJira.API.Factory;
using PluginJira.API.Utility.EndpointHelperEndpoints;
using PluginJira.DataContracts;
using PluginJira.Helper;
using RestSharp;

namespace PluginJira.API.Utility
{
    public static class EndpointHelper
    {
        public static readonly string LinksPropertyId = "Links";
        public static readonly string CustomFieldsId = "CustomFields";
        private static readonly Dictionary<string, Endpoint> Endpoints = new Dictionary<string, Endpoint>();

        static EndpointHelper()
        {
            IssuesEndpointHelper.IssuesEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            ProjectsEndpointHelper.ProjectsEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
            TimeTrackingEndpointsHelper.TimeTrackingEndpoints.ToList().ForEach(x => Endpoints.TryAdd(x.Key, x.Value));
        }

        public static Dictionary<string, Endpoint> GetAllEndpoints()
        {
            return Endpoints;
        }

        public static Endpoint? GetEndpointForId(string id)
        {
            return Endpoints.ContainsKey(id) ? Endpoints[id] : null;
        }

        public static Endpoint? GetEndpointForSchema(Schema schema)
        {
            var endpointMetaJson = JsonConvert.DeserializeObject<dynamic>(schema.PublisherMetaJson);
            string endpointId = endpointMetaJson.Id;
            return GetEndpointForId(endpointId);
        }
        public static void FlattenAndReadRecord(Dictionary<string, object> recordMap, JObject thisObject, int depth, string runningColumnName)
        {
            if (depth <= 0)
            {
                recordMap[$"{runningColumnName.TrimEnd('.')}"] = thisObject.ToString();
                return;
            }
            try
            {
                foreach (var child in thisObject)
                {
                    var hasChildren = false;
                    try
                    {
                        //checking if castable / checking for empty lists
                        var childValue = (JObject) child.Value;
                        hasChildren = child.Value?.Children().Count() > 0;
                    }
                    catch (Exception e)
                    {
                        hasChildren = false;
                    }
                    
                    if (hasChildren)
                    {
                        FlattenAndReadRecord(recordMap, (JObject)child.Value, depth - 1, $"{runningColumnName}.{child.Key}");
                    }
                    else
                    {
                        recordMap[$"{runningColumnName.TrimEnd('.')}.{child.Key}"] = child.Value?.ToString() ?? "";
                    }
                }
            }
            catch (Exception e)
            {
                            
            }
        }
    }

    public abstract class Endpoint
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string BasePath { get; set; } = "";
        public string AllPath { get; set; } = "";
        public string? DetailPath { get; set; }
        public string? DetailPropertyId { get; set; }
        public List<string> PropertyKeys { get; set; } = new List<string>();

        public virtual bool ShouldGetStaticSchema { get; set; } = false;

        protected virtual string WritePathPropertyId { get; set; } = "";

        protected virtual List<string> RequiredWritePropertyIds { get; set; } = new List<string>
        {
        };

        public List<EndpointActions> SupportedActions { get; set; } = new List<EndpointActions>();

        public virtual async Task<Count> GetCountOfRecords(IApiClientFactory factory, Settings settings)
        {
            var response = await factory.CreateApiClient(settings).GetAsync($"{BasePath.TrimEnd('/')}/{AllPath.TrimStart('/')}");

            var recordsList = JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());

            return new Count
            {
                Kind = Count.Types.Kind.Exact,
                Value = (int) recordsList.TotalRecords
            };
        }

        public virtual IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings,
            DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
        {

            throw new NotImplementedException();


        }

        public virtual async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
            IServerStreamWriter<RecordAck> responseStream)
        {
            throw new NotImplementedException();
        }

        public virtual Task<Schema> GetStaticSchemaAsync(IApiClientFactory factory, Settings settings, Schema schema)
        {
            throw new System.NotImplementedException();
        }

        public virtual Task<bool> IsCustomProperty(IApiClientFactory factory, Settings settings, string propertyId)
        {
            return Task.FromResult(false);
        }

        public Schema.Types.DataFlowDirection GetDataFlowDirection()
        {
            if (CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.ReadWrite;
            }

            if (CanRead() && !CanWrite())
            {
                return Schema.Types.DataFlowDirection.Read;
            }

            if (!CanRead() && CanWrite())
            {
                return Schema.Types.DataFlowDirection.Write;
            }

            return Schema.Types.DataFlowDirection.Read;
        }


        private bool CanRead()
        {
            return SupportedActions.Contains(EndpointActions.Get);
        }

        private bool CanWrite()
        {
            return SupportedActions.Contains(EndpointActions.Post) ||
                   SupportedActions.Contains(EndpointActions.Put) ||
                   SupportedActions.Contains(EndpointActions.Delete);
        }
    }

    public enum EndpointActions
    {
        Get,
        Post,
        Put,
        Delete
    }
}