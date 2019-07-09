using Newtonsoft.Json;

namespace Base.Service
{
    [JsonConverter(typeof(StringJsonConverter))]
    public class StringData
    {
        public string Value { get; set; }
    }
}
