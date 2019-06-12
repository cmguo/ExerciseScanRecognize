using Newtonsoft.Json;
using System.Collections.Generic;

namespace Exercise.Algorithm
{
    public class AnswerData
    {
        [JsonPropertyAttribute(Required = Required.Default)]
        public byte[] redressedImgBytes { get; set; }

        [JsonPropertyAttribute(Required = Required.Default)]
        public string imageName { get; set; } // 提供给中台的名称（文件数据的Md5）

        public int imgWidth { get; set; }
        public int imgHeight { get; set; }
        public IList<AreaInfo> areaInfo { get; set; }
        public class ItemInfo
        {
            public int statusOfItem { get; set; } // 0 SUCCESS 1 检测结果异常
            public Location itemLocation { get; set; } // 相对于图像左上角的位置，绝对位置
            public string analyzeResult { get; set; }
        }

        public class QuestionInfo
        {
            public string questionId { get; set; }
            public Location questionLocation { get; set; }
            public int numOfRectsToDetectInQuestion { get; set; }
            public IList<ItemInfo> itemInfo { get; set; }
        }

        public class AreaInfo
        {
            public int areaId { get; set; }
            public AreaType areaType { get; set; }
            public int numOfQuestions { get; set; }
            public Location areaLocation { get; set; }
            public IList<QuestionInfo> questionInfo { get; set; }
        }

    }
}
