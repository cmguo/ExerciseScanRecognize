using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    class ByteArrayJsonConverter : JsonConverter<byte[]>
    {
        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            List<byte> bytes = new List<byte>();
            int? b;
            while ((b = reader.ReadAsInt32()) != null)
            {
                bytes.Add((byte)b);
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
