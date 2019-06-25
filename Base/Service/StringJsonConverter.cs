using Newtonsoft.Json;
using System;

namespace Base.Service
{
    class StringJsonConverter : JsonConverter<StringData>
    {
        public override StringData ReadJson(JsonReader reader, Type objectType, StringData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new StringData() { Value = (string) reader.Value };
        }

        public override void WriteJson(JsonWriter writer, StringData value, JsonSerializer serializer)
        {
            //writer.WriteValue(System.Convert.ToBase64String(value));
            writer.WriteRawValue(value.Value);
        }
    }
}
