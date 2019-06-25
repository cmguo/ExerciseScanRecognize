using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service
{
    [JsonConverter(typeof(StringJsonConverter))]
    public class StringData
    {
        public string Value { get; set; }
    }
}
