using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public IList<Marker> PaperMarkers { get; set; }
        public IList<Marker> AreaMarkers { get; set; }
        public IList<Area> AreaInfo { get; set; }

        [JsonIgnore]
        public Location QRCodeLocation => GetQRCodeLocation();
        [JsonIgnore]
        public Location AreaLocation => GetAreaLocation();

        private Location GetAreaLocation()
        {
            if (AreaMarkers == null || AreaMarkers.Count < 2)
                return null;
            return AreaMarkers.Select(a => a.MarkerLocation).Aggregate((l, r) => new Location()
            {
                LeftTop = new Point() { X = Math.Min(l.LeftTop.X, r.LeftTop.X), Y = Math.Min(l.LeftTop.Y, r.LeftTop.Y) },
                RightBottom = new Point() { X = Math.Max(l.RightBottom.X, r.RightBottom.X), Y = Math.Max(l.RightBottom.Y, r.RightBottom.Y) },
            });
        }

        private Location GetQRCodeLocation()
        {
            if (PaperMarkers == null || PaperMarkers.Count < 2)
                return null;
            return new Location()
            {
                LeftTop = new Point()
                {
                    X = PaperMarkers[0].MarkerLocation.RightBottom.X,
                    Y = PaperMarkers[0].MarkerLocation.LeftTop.Y
                }, 
                RightBottom = new Point
                {
                    X = PaperMarkers[1].MarkerLocation.LeftTop.X,
                    Y = PaperMarkers[1].MarkerLocation.RightBottom.Y
                }
            };
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
            public int Index { get; set; } // 0 SUCCESS 1 检测结果异常
            public PagingInfo PagingInfo { get; set; }
            public int StatusOfItem { get; set; } // 0 SUCCESS 1 检测结果异常
            public Location ItemLocation { get; set; } // 相对于图像左上角的位置，绝对位置
            public IList<Result> AnalyzeResult { get; set; }
            public double Score { get; set; }

            public void ApplyFrom(PageData.Item item)
            {
                if (!_additionalData.ContainsKey("value"))
                    _additionalData.Add("value", item.Value);
                if (!_additionalData.ContainsKey("halfScore"))
                    _additionalData.Add("halfScore", item.HalfScore);
                if (!_additionalData.ContainsKey("totalScore"))
                    _additionalData.Add("totalScore", item.TotalScore);
                foreach (var d in item._additionalData)
                {
                    if (!_additionalData.ContainsKey(d.Key))
                        _additionalData.Add(d);
                }
            }

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

            public void ApplyFrom(PageData.Question question)
            {
                if (!_additionalData.ContainsKey("value"))
                    _additionalData.Add("index", question.Index);
                if (!_additionalData.ContainsKey("questionType"))
                    _additionalData.Add("questionType", (int) question.QuestionType);
                foreach (var d in question._additionalData)
                {
                    if (!_additionalData.ContainsKey(d.Key))
                        _additionalData.Add(d);
                }
            }

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

            public void ApplyFrom(PageData.Area area)
            {
                foreach (var d in area._additionalData)
                {
                    if (!_additionalData.ContainsKey(d.Key))
                        _additionalData.Add(d);
                }
            }

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
