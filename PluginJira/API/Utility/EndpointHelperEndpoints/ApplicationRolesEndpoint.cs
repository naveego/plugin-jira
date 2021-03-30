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
    public static class ApplicationRolesEndpointHelper
    {
        private class ApplicationRolesEndpoint : Endpoint
        {
            public override async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings,
            DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null, bool isDiscoverRead = false)
            {
                // fetch all records
                var jira = factory.CreateApiClient(settings);

                var response = await jira.GetAsync("applicationrole");
                var recordsList =JsonConvert.DeserializeObject<DataWrapper>(await response.Content.ReadAsStringAsync());


                foreach (var item in recordsList.Items){

                    var recordMap = new Dictionary<string, object>();
                    recordMap.TryAdd(item.ToString(), item.Values);

                        
                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }

                }

        }
        
        public static readonly Dictionary<string, Endpoint> ApplicationRolesEndpoints = new Dictionary<string, Endpoint>
        {
            {"AllApplicationRoles", new ApplicationRolesEndpoint
            {
                Id = "AllApplicationRoles",
                Name = "All Application Roles",
                BasePath = "/applicationrole",
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