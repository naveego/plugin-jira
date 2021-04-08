using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginJira.DataContracts
{
    public class ApplicationEndpointWrapper
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("defaultGroups")]
        public List<string> DefaultGroups { get; set; }

        [JsonProperty("selectedByDefault")]
        public bool SelectedByDefault { get; set; }

        [JsonProperty("defined")]
        public bool Defined { get; set; }

        [JsonProperty("numberOfSeats")]
        public long NumberOfSeats { get; set; }

        [JsonProperty("remainingSeats")]
        public long RemainingSeats { get; set; }

        [JsonProperty("userCount")]
        public long UserCount { get; set; }

        [JsonProperty("userCountDescription")]
        public string UserCountDescription { get; set; }

        [JsonProperty("hasUnlimitedSeats")]
        public bool HasUnlimitedSeats { get; set; }

        [JsonProperty("platform")]
        public bool Platform { get; set; }

    }
}