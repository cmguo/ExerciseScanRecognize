using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Exercise.Algorithm
{
    class ByteArrayJsonConverter : JsonConverter<byte[]>
    {
        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            List<byte> bytes = new List<byte>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.Integer)
                    bytes.Add((byte)(Int64)reader.Value);
                else if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
            }
            return bytes.ToArray();
        }

        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        {
            //writer.WriteValue(System.Convert.ToBase64String(value));
            writer.WriteStartArray();
            foreach (byte b in value)
                writer.WriteRawValue(((int)(b)).ToString());
            writer.WriteEndArray();
        }
    }
}
