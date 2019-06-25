using Newtonsoft.Json;
using System;

namespace TalBase.Service
{
    class NothingJsonConverter : JsonConverter<Nothing>
    {
        public override Nothing ReadJson(JsonReader reader, Type objectType, Nothing existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, Nothing value, JsonSerializer serializer)
        {
            writer.WriteNull();
        }
    }
}
