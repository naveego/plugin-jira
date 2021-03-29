using System.Net.Http;
using PluginJira.Helper;
using Atlassian.Jira;

namespace PluginJira.API.Factory
{
    public class ApiClientFactory: IApiClientFactory
    {
        private HttpClient Client { get; set; }

        public ApiClientFactory(HttpClient client)
        {
            Client = client;
        }

        public IApiClient CreateApiClient(Settings settings)
        {
            return new ApiClient(Client, settings);
        }

        public Jira CreateJiraClient(Settings settings)
        {
            return Jira.CreateRestClient(settings.GetSdkUri(), settings.Username, settings.ApiKey);
        }
    }
}