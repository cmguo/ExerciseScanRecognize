using Newtonsoft.Json;
using System.Collections.Generic;

namespace Exercise.Algorithm
{
    public class PageData
    {
        [JsonPropertyAttribute(Required = Required.Default)]
        public byte[] imgBytes { get; set; }
        public int numOfAreaMarkers { get; set; }
        public IList<AreaInfo> areaInfo { get; set; }
        public class ItemInfo
        {
            public int totalScore { get; set; }
            // 选择题的选项信息，以英文逗号分隔 "A,B,C,D"
            // 填空题该字段为空
            // 解答题每小题的分值信息（按从左到右顺序给到，以英文逗号分隔）
            public string value { get; set; }
            public Location itemLocation { get; set; }
        }

        public class QuestionInfo
        {
            public string questionId { get; set; }
            public Location questionLocation { get; set; } // 相对页面左上角黑色定位块左上点的位置信息，相对位置
            public int numOfRectsToDetectInQuestion { get; set; }
            public IList<ItemInfo> itemInfo { get; set; }
        }

        public class AreaInfo
        {
            public int areaId { get; set; }
            public AreaType areaType { get; set; }
            public int numOfQuestions { get; set; }
            public PagingInfo pagingInfo { get; set; }
            public Location areaLocation { get; set; } // 相对于图像左上角的位置，相对位置
            public IList<QuestionInfo> questionInfo { get; set; }
        }

    }
}
