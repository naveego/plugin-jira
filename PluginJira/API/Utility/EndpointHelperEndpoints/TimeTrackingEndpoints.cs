using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Jira;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginJira.API.Factory;
using PluginJira.DataContracts;
using PluginJira.Helper;

namespace PluginJira.API.Utility.EndpointHelperEndpoints
{
    public static class TimeTrackingEndpointsHelper
    {
        private class TimeTrackingEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings,
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
                        try
                        {
                            recordMap["Id"] = issue.Id;
                            recordMap["Key"] = issue.Key;
                            recordMap["Self"] = issue.Self;
                            recordMap["timeoriginalestimate"] = issue.Fields["timeoriginalestimate"].ToString() ?? "";
                            recordMap["aggregatetimeoriginalestimate"] = issue.Fields["aggregatetimeoriginalestimate"].ToString() ?? "";
                            recordMap["timespent"] = issue.Fields["timespent"].ToString() ?? "";
                            recordMap["aggregatetimespent"] = issue.Fields["aggregatetimespent"].ToString() ?? "";

                            try
                            {
                                var progress = JObject.Parse(issue.Fields["progress"].ToString() ?? "");
                                recordMap["progress.progress"] = progress["progress"]?.ToString() ?? "";
                                recordMap["progress.total"] = progress["total"]?.ToString() ?? "";
                                recordMap["progress.percent"] = progress["percent"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["progress.progress"] = "";
                                recordMap["progress.total"] = "";
                                recordMap["progress.percent"] = "";
                            }
                            
                            try
                            {
                                var timetracking = JObject.Parse(issue.Fields["timetracking"].ToString() ?? "");
                                recordMap["timetracking.remainingEstimate"] = timetracking["remainingEstimate"]?.ToString() ?? "";
                                recordMap["timetracking.timeSpent"] = timetracking["timeSpent"]?.ToString() ?? "";
                                recordMap["timetracking.remainingEstimateSeconds"] = timetracking["remainingEstimateSeconds"]?.ToString() ?? "";
                                recordMap["timetracking.timeSpentSeconds"] = timetracking["timeSpentSeconds"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["timetracking.remainingEstimate"] = "";
                                recordMap["timetracking.timeSpent"] = "";
                                recordMap["timetracking.remainingEstimateSeconds"] = "";
                                recordMap["timetracking.timeSpentSeconds"] = "";
                            }
                            
                            
                            try
                            {
                                var priority = JObject.Parse(issue.Fields["priority"].ToString() ?? "");
                                recordMap["priorityName"] = priority["name"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["priorityName"] = "";
                            }
                            
                            try
                            {
                                var status = JObject.Parse(issue.Fields["status"].ToString() ?? "");
                                recordMap["status.description"] = status["description"]?.ToString() ?? "";
                                recordMap["status.name"] = status["name"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["status.description"] = "";
                                recordMap["status.name"] = "";
                            }
                            
                            try
                            {
                                var creator = JObject.Parse(issue.Fields["creator"].ToString() ?? "");
                                recordMap["creator.emailAddress"] = creator["emailAddress"]?.ToString() ?? "";
                                recordMap["creator.displayName"] = creator["displayName"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["creator.emailAddress"] = "";
                                recordMap["creator.displayName"] = "";
                            }
                            
                            try
                            {
                                var aggregateProgress = JObject.Parse(issue.Fields["aggregateprogress"].ToString() ?? "");
                                recordMap["aggregateProgress.progress"] = aggregateProgress["progress"]?.ToString() ?? "";
                                recordMap["aggregateProgress.total"] = aggregateProgress["total"]?.ToString() ?? "";
                                recordMap["aggregateProgress.percent"] = aggregateProgress["percent"]?.ToString() ?? "";
                            }
                            catch
                            {
                                recordMap["aggregateProgress.progress"] = "";
                                recordMap["aggregateProgress.total"] = "";
                                recordMap["aggregateProgress.percent"] = "";
                            }
                        }
                        catch (Exception e)
                        {
                            var db = recordMap;
                        }
                        

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                }
            }

            public override async Task<Schema> GetStaticSchemaAsync(IApiClientFactory factory, Settings settings, Schema schema)
            {
                List<string> staticSchemaProperties = new List<string>()
                {
                    "Id",
                    "Key",
                    "Self",
                    "timeoriginalestimate",
                    "aggregatetimeoriginalestimate",
                    "timespent",
                    "aggregatetimespent",
                    "progress.progress",
                    "progress.total",
                    "progress.percent",
                    "timetracking.remainingEstimate",
                    "timetracking.timeSpent",
                    "timetracking.remainingEstimateSeconds",
                    "timetracking.timeSpentSeconds",
                    "priorityName",
                    "status.description",
                    "status.name",
                    "creator.emailAddress",
                    "creator.displayName",
                    "aggregateProgress.progress",
                    "aggregateProgress.total",
                    "aggregateProgress.percent"
                };

                var properties = new List<Property>();

                foreach (var staticProperty in staticSchemaProperties)
                {
                    var property = new Property();

                    property.Id = staticProperty;
                    property.Name = staticProperty;

                    switch (staticProperty)
                    {
                        case("Id"):
                            property.IsKey = true;
                            property.TypeAtSource = "string";
                            property.Type = PropertyType.String;
                            break;
                        default:
                            property.IsKey = false;
                            property.TypeAtSource = "string";
                            property.Type = PropertyType.String;
                            break;
                            
                    }
                    properties.Add(property);
                }
                schema.Properties.Clear();
                schema.Properties.AddRange(properties);

                schema.DataFlowDirection = GetDataFlowDirection();
                return schema;
            }
        }
        public static readonly Dictionary<string, Endpoint> TimeTrackingEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllTimeTracking", new TimeTrackingEndpoint()
            {
                Id = "AllTimeTracking",
                Name = "All Time Tracking",
                BasePath = "/TimeTracking",
                AllPath = "/",
                DetailPath = "/",
                DetailPropertyId = "",
                SupportedActions = new List<EndpointActions>
                {
                    EndpointActions.Get
                },
                ShouldGetStaticSchema = true,
            }},
        };
    }
}