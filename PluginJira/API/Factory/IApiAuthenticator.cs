using System.Threading.Tasks;

namespace PluginJira.API.Factory
{
    public interface IApiAuthenticator
    {
        Task<string> GetToken();
    }
}