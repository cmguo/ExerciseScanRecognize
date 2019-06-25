using Newtonsoft.Json;

namespace TalBase.Service
{
    [JsonConverter(typeof(NothingJsonConverter))]
    public class Nothing
    {
    }
}