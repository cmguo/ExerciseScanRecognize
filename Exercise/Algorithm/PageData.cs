using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Exercise.Algorithm
{
    public class PageData
    {
        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        [Obsolete("ImgBytes is deprecated, please use ImgPathIn & ImgPathOut instead.")]
        public byte[] ImgBytes { get; set; }

        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ImgPathIn { get; set; }
        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ImgPathOut { get; set; }

        public int NumOfAreaMarkers { get; set; }
        public IList<Area> AreaInfo { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData
             = new Dictionary<string, JToken>();

        public class Item
        {
            // public float TotalScore { get; set; }
            public int Index { get; set; }
            public PagingInfo PagingInfo { get; set; }
            // 选择题的选项信息，以英文逗号分隔 "A,B,C,D"
            // 填空题该字段为空
            // 解答题每小题的分值信息（按从左到右顺序给到，以英文逗号分隔）
            public string Value { get; set; }
            public float TotalScore { get; set; }
            public Location ItemLocation { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Question
        {
            public int Index { get; set; }
            public string QuestionId { get; set; }
            public Location QuestionLocation { get; set; } // 相对页面左上角黑色定位块左上点的位置信息，相对位置
            //public int NumOfRectsToDetectInQuestion { get; set; }
            public IList<Item> ItemInfo { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

        public class Area
        {
            public int AreaId { get; set; }
            public AreaType AreaType { get; set; }
            //public int NumOfQuestions { get; set; }
            public PagingInfo PagingInfo { get; set; }
            public Location AreaLocation { get; set; } // 相对于图像左上角的位置，相对位置
            public IList<Question> QuestionInfo { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData
                 = new Dictionary<string, JToken>();
        }

    }
}
