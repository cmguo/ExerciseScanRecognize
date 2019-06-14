using Newtonsoft.Json;
using System.Collections.Generic;

namespace Exercise.Algorithm
{
    public class AnswerData
    {
        [JsonPropertyAttribute(Required = Required.Default)]
        public byte[] RedressedImgBytes { get; set; }

        [JsonPropertyAttribute(Required = Required.Default)]
        public string ImageName { get; set; } // 提供给中台的名称（文件数据的Md5）

        [JsonPropertyAttribute(Required = Required.Default)]
        public int PageId { get; set; } // 页面编号，可能缺页

        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }
        public IList<Area> AreaInfo { get; set; }

        public class Item
        {
            public int StatusOfItem { get; set; } // 0 SUCCESS 1 检测结果异常
            public Location ItemLocation { get; set; } // 相对于图像左上角的位置，绝对位置
            public string AnalyzeResult { get; set; }
        }

        public class Question
        {
            public string QuestionId { get; set; }
            public Location QuestionLocation { get; set; }
            public int NumOfRectsToDetectInQuestion { get; set; }
            public IList<Item> ItemInfo { get; set; }
        }

        public class Area
        {
            public int AreaId { get; set; }
            public AreaType AreaType { get; set; }
            public int NumOfQuestions { get; set; }
            public Location AreaLocation { get; set; }
            public IList<Question> QuestionInfo { get; set; }
        }

    }
}
