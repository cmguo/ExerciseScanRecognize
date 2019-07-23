using Newtonsoft.Json;
using System;

namespace Exercise.Algorithm
{
    public class PageRaw
    {
        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        [Obsolete("ImgBytes is deprecated, please use ImgPathIn instead.")]
        public byte[] ImgBytes { get; set; }

        public string ImgPathIn { get; set; }
    }
}
