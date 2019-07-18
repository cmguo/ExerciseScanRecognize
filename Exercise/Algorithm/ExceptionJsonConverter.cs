using Newtonsoft.Json;
using System;

namespace Exercise.Algorithm
{
    class ExceptionJsonConverter : JsonConverter<Exception>
    {
        public override Exception ReadJson(JsonReader reader, Type objectType, Exception existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            if (reader.TokenType == JsonToken.String)
                return new Exception(reader.Value as string);
            return null;
        }

        public override void WriteJson(JsonWriter writer, Exception value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Message);
        }
    }
}
