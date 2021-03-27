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

                // var issues = jira.Issues.Queryable
                // .Select(i => i)
                // .GroupBy(i => i.Key);

                var issues = from i in jira.Issues.Queryable
                    select i;

                // iterate and return each record
                // foreach on results of JQL
                foreach (var issue in issues) 
                {
                    var recordMap = new Dictionary<string, object>();

                    // pull in all desired properties
                    recordMap["Key"] = issue.Key.Value;     

                    yield return new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(recordMap)
                    };
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