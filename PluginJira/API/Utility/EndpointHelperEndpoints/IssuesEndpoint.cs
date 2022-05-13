using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginJira.API.Factory;
using PluginJira.API.Utility.EndpointHelperEndpoints;
using PluginJira.DataContracts;
using PluginJira.Helper;
using RestSharp;
using JsonSerializer = RestSharp.Serialization.Json.JsonSerializer;

namespace PluginJira.API.Utility.EndpointHelperEndpoints
{
    public static class IssuesEndpointHelper
    {
        private class IssuesEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory,
                Settings settings,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                

                var httpClient = factory.CreateApiClient(settings);
                
                var depth = Int32.Parse(await httpClient.GetDepth());

                var hasMore = true;
                var startAt = 0;
                var maxResults = 100;
                
                while (hasMore)
                {
                    var json = $"{{\"jql\": \"\",\"startAt\": {startAt}," +
                               $"\"maxResults\": {maxResults}," +
                               "\"validateQuery\": true," +
                               "\"fields\": " +
                               "[\"*all\", \"-comment\",\"-attachment\",\"-issuelinks\",\"-subtasks\",\"-watches\",\"-worklog\"]}";
                    var result = await httpClient.PostAsync("search",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                    var resultString = await result.Content.ReadAsStringAsync();
                    var issuesResponseWrapper = JsonConvert.DeserializeObject<IssuesWrapper>(resultString);

                    startAt += issuesResponseWrapper.Issues.Count();
                    if (startAt >= issuesResponseWrapper.Total || (isDiscoverRead && startAt >= 100))
                    {
                        hasMore = false;
                    }
                    
                    foreach (var issue in issuesResponseWrapper.Issues)
                    {
                        var recordMap = new Dictionary<string, object>();

                        recordMap["Id"] = issue.Id;
                        recordMap["Key"] = issue.Key;
                        recordMap["Self"] = issue.Self;
                        foreach (var field in issue.Fields)
                        {
                            if (depth > 0)
                            {
                                try
                                {
                                    JObject parentObject = (JObject) field.Value;
                                    EndpointHelper.FlattenAndReadRecord(recordMap, parentObject, depth, field.Key);
                                }
                                catch (Exception e)
                                {
                                    var db = e;
                                }
                                finally
                                {
                                    //do entire submission of the object to single row for referencing
                                    recordMap[field.Key] = field.Value?.ToString() ?? "";
                                }
                            }
                            else
                            {
                                recordMap[field.Key] = field.Value?.ToString() ?? "";
                            }

                        }

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                }
            } 
        }
        
        public static readonly Dictionary<string, Endpoint> IssuesEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllIssues", new IssuesEndpoint
            {
                Id = "AllIssues",
                Name = "All Issues",
                BasePath = "/Issues",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                PropertyKeys = new List<string>
                {
                    "BounceID"
                }
            }},
        };
    }
}