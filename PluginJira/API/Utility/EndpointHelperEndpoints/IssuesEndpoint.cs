using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.API.Utility.EndpointHelperEndpoints;
using PluginJira.DataContracts;
using PluginJira.Helper;

namespace PluginJira.API.Utility.EndpointHelperEndpoints
{
    public static class IssuesEndpointHelper
    {
        private class IssuesEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings,
            DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                // fetch all records
                var jira = factory.CreateJiraClient(settings);

                var projectKey = settings.GetProject();

                // Adding logic for pagination
                var itemsPerPage = 50;

                var startAt = 0;

                while (true)
                {
                    var issues = await jira.Issues.GetIssuesFromJqlAsync($"project = {projectKey} ORDER BY created DESC", itemsPerPage, startAt);
                   
                    if (issues.Count() == 0)
                    {
                        break;
                    }
                        
                    // iterate and return each record
                    // foreach on results of JQL
                    foreach (var issue in issues) 
                    {
                        var recordMap = new Dictionary<string, object>();

                        // pull in all desired properties
                        recordMap["Key"] = Convert.ToString(issue.Key.Value);
                        recordMap["Project"] = Convert.ToString(issue.Project);
                        recordMap["Issuetype"] = Convert.ToString(issue.Type.Name);
                        recordMap["Description"] = Convert.ToString(issue.Description);
                        recordMap["Reporter"] = Convert.ToString(issue.ReporterUser.DisplayName);
                        recordMap["Created"] = Convert.ToString(issue.Created.Value);
                        recordMap["Status"] = Convert.ToString(issue.Status.Name);
                        recordMap["Resolution"] = Convert.ToString(issue.Resolution);
                        recordMap["Updated"] = Convert.ToString(issue.Updated.Value);

                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }

                    startAt += itemsPerPage;
                }
            } 
        
            public override async Task<Count> GetCountOfRecords(IApiClientFactory factory, Settings settings)
            {
                var jira = factory.CreateJiraClient(settings);

                var projectKey = settings.GetProject();

                var issues = await jira.Issues.GetIssuesFromJqlAsync($"project = {projectKey} ORDER BY created DESC");

                var total = issues.TotalItems;

                return new Count
                {
                    Kind = Count.Types.Kind.Exact,
                    Value = (int) total
                };
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