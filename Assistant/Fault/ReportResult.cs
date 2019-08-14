using Newtonsoft.Json;
using System.Collections.Generic;

namespace Assistant.Fault
{
    public class ReportResult
    {
        public string Id { get; set; }
        [JsonProperty("map")]
        public IDictionary<string, string> FilePostUrls { get; set; }

    }
}
