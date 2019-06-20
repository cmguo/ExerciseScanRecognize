﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class PageRaw
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] ImgBytes { get; set; }
    }
}
