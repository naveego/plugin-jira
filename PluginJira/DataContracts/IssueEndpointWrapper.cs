using System.Collections.Generic;

namespace PluginJira.DataContracts
{
    public class IssueEndpointWrapper
    {
        public long PageSize { get; set; }
        public long PageNumber { get; set; }
        public long TotalRecords { get; set; }
        public long TotalPages { get; set; }

    }
}