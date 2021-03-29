using PluginJira.Helper;
using Atlassian.Jira;

namespace PluginJira.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(Settings settings);

        Jira CreateJiraClient(Settings settings);
    }
}