using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
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
                // fetch all records
                var jira = factory.CreateJiraClient(settings);

                // var issues = jira.Issues.Queryable
                // .Select(i => i)
                // .GroupBy(i => i.Key);

                var issues = from i in jira.Issues.Queryable
                    select i;
                
                var issuesExist = false;
                try
                {
                    issuesExist = issues.Any();
                }
                catch
                {
                    issuesExist = false;
                }
                
                // iterate and return each record
                // foreach on results of JQL
                if (issuesExist)
                {
                    foreach (var issue in issues)
                    {
                        if (issue.TimeTrackingData == null)
                        {
                            continue;
                        }

                        var recordMap = new Dictionary<string, object>();

                        recordMap["IssueKey"] = issue.Key.Value;
                        recordMap["IssueProject"] = issue.Project;
                        recordMap["IssueStatus"] = issue.Status.Name;

                        recordMap["OriginalEstimate"] = issue.TimeTrackingData.OriginalEstimate ?? "";
                        recordMap["OriginalEstimateInSeconds"] =
                            issue.TimeTrackingData.OriginalEstimateInSeconds.ToString() ?? "";
                        recordMap["RemainingEstimate"] = issue.TimeTrackingData.RemainingEstimate ?? "";
                        recordMap["RemainingEstimateInSeconds"] =
                            issue.TimeTrackingData.RemainingEstimateInSeconds.ToString() ?? "";
                        recordMap["TimeSpent"] = issue.TimeTrackingData.TimeSpent ?? "";
                        recordMap["TimeSpentInSeconds"] = issue.TimeTrackingData.TimeSpentInSeconds.ToString() ?? "";

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                }
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
                PropertyKeys = new List<string>
                {
                    "BounceID"
                }
            }},
        };
    }
}