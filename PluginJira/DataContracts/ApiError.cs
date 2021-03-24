using Newtonsoft.Json;

namespace PluginJira.DataContracts
{
    public class ApiError
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}