using Newtonsoft.Json;

namespace Exercise.Algorithm
{
    public class PageRaw
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] ImgBytes { get; set; }
    }
}
