using System.Collections.Generic;

namespace Assistant.Fault
{
    public class ReportResult
    {
        public string Id { get; set; }
        public IDictionary<string, string> FilePostUrls { get; set; }

    }
}
