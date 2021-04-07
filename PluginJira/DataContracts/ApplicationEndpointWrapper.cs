using System.Collections.Generic;

namespace PluginJira.DataContracts
{
    public class ApplicationEndpointWrapper
    {
        public string key { get; set; }

        public List<string> groups { get; set; }

        public string name { get; set; }
        public List<string> defaultGroups { get; set; }

        public bool selectedByDefault { get; set; }

        public bool defined { get; set; }

        public long numberOfSeats { get; set; }
        public long remainingSeats { get; set; }
        public long userCount { get; set; }
        public string userCountDescription { get; set; }
        public bool hasUnlimitedSeats { get; set; }
        public bool platform { get; set; }

    }
}