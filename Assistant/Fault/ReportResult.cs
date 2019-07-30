using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Fault
{
    public class ReportResult
    {
        public string Id { get; set; }
        public IList<string> FilePostUrls { get; set; }

    }
}
