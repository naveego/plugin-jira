using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.Helper;

namespace PluginJira.API.Utility.EndpointHelperEndpoints
{
    public static class ProjectsEndpointHelper
    {
        private class ProjectsEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings,
                DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                // fetch all records
                var jira = factory.CreateJiraClient(settings);

                // var issues = jira.Issues.Queryable
                // .Select(i => i)
                // .GroupBy(i => i.Key);

                var projects = await jira.Projects.GetProjectsAsync();

                // iterate and return each record
                // foreach on results of JQL
                foreach (var project in projects)
                {
                    var recordMap = new Dictionary<string, object>();

                    // pull in all desired properties
                    recordMap["Id"] = project.Id ?? "";
                    recordMap["Name"] = project.Name ?? "";
                    recordMap["Key"] = project.Key ?? "";
                    recordMap["Category.Id"] = project.Category?.Id ?? "";
                    recordMap["Category.Name"] = project.Category?.Name ?? "";
                    recordMap["Category.Description"] = project.Category?.Description ?? "";
                    recordMap["URL"] = project.Url ?? "";
                    recordMap["Lead"] = project.Lead ?? "";
                    recordMap["LeadUser.Key"] = project.LeadUser?.Key ?? "";
                    recordMap["LeadUser.Email"] = project.LeadUser?.Email ?? "";
                    recordMap["LeadUser.Locale"] = project.LeadUser?.Locale ?? "";
                    recordMap["LeadUser.Username"] = project.LeadUser?.Username ?? "";
                    recordMap["LeadUser.AccountId"] = project.LeadUser?.AccountId ?? "";
                    recordMap["LeadUser.DisplayName"] = project.LeadUser?.DisplayName ?? "";
                    recordMap["LeadUser.IsActive"] = project.LeadUser?.IsActive.ToString() ?? "";


                    yield return new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(recordMap)
                    };
                }
            } 
        }
        
        public static readonly Dictionary<string, Endpoint> ProjectsEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllProjects", new ProjectsEndpoint
            {
                Id = "AllProjects",
                Name = "AllProjects",
                BasePath = "/Projects",
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