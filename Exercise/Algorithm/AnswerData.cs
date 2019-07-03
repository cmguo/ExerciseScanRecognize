using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace Exercise.Algorithm
{
    public class AnswerData
    {
        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] RedressedImgBytes { get; set; }

        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ImageName { get; set; } // 提供给中台的名称（文件数据的Md5）

        [JsonProperty(Required = Required.Default)]
        public int PageId { get; set; } // 页面编号，可能缺页

        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }

        //public IList<Marker> PaperMarkers { get; set; }
        //public IList<Marker> AreaMarkers { get; set; }
        public IList<Area> AreaInfo { get; set; }


        [JsonIgnore]
        public IList<Question> AnswerExceptions { get; set; }

        [JsonIgnore]
        public IList<Question> CorrectionExceptions { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            AreaInfo.SelectMany(a => a.QuestionInfo).All(q => { q.HasException = q.ItemInfo.Any(i => i.StatusOfItem != 0); return true; });
            AnswerExceptions = AreaInfo.Where(a => a.AreaType == AreaType.SingleChoice)
                .SelectMany(a => a.QuestionInfo.Where(q => q.HasException)).ToList();
            if (AnswerExceptions.Count == 0)
                AnswerExceptions = null;
            CorrectionExceptions = AreaInfo.Where(a => a.AreaType == AreaType.Answer)
                .SelectMany(a => a.QuestionInfo.Where(q => q.HasException)).ToList();
            if (CorrectionExceptions.Count == 0)
                CorrectionExceptions = null;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData
             = new Dictionary<string, JToken>();

        public class Result
        {
            public string Value { get; set; }
            public Location ValueLocation { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Item
        {
            public int StatusOfItem { get; set; } // 0 SUCCESS 1 检测结果异常
            public Location ItemLocation { get; set; } // 相对于图像左上角的位置，绝对位置
            public IList<Result> AnalyzeResult { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Question
        {
            public string QuestionId { get; set; }
            public PagingInfo PagingInfo { get; set; }
            public Location QuestionLocation { get; set; }
            public IList<Item> ItemInfo { get; set; }

            [JsonIgnore]
            public bool HasException { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Area
        {
            public int AreaId { get; set; }
            public AreaType AreaType { get; set; }
            public Location AreaLocation { get; set; }
            public IList<Question> QuestionInfo { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Marker
        {
            public Location MarkerLocation { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

    }
}
