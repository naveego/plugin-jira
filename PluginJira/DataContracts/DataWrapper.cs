using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginJira.DataContracts
{
    public class DataWrapper
    {
        public long PageSize { get; set; }
        public long PageNumber { get; set; }
        public long TotalRecords { get; set; }
        public long TotalPages { get; set; }
        public List<Dictionary<string, object>>? Items { get; set; }
    }
    public class IssuesWrapper
    {
        [JsonProperty("issues")]
        public List<Issue> Issues { get; set; }
        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }
        [JsonProperty("startAt")]
        public int StartAt { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Issue
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("self")]
        public string Self { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("fields")]
        public Dictionary<string, object> Fields { get; set; }
    }
}