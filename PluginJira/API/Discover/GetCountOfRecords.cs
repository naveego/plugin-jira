using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.API.Utility;
using PluginJira.Helper;

namespace PluginJira.API.Discover
{
    public static partial class Discover
    {
        public static Task<Count> GetCountOfRecords(IApiClientFactory factory, Settings settings, Endpoint? endpoint)
        {
            return endpoint != null
                ? endpoint.GetCountOfRecords(factory, settings)
                : Task.FromResult(new Count {Kind = Count.Types.Kind.Unavailable});
        }
    }
}