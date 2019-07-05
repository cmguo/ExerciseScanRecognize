using Exercise.Algorithm;
using System;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class SubmitData
    {
        public String HomeworkId { get; set; } // 批次记录ID，后端生成
        public String PaperId { get; set; }
        public bool Finished { get; set; }
        public IList<AnswerInfo> Data { get; set; }

        public class AnswerInfo
        {
            public String StudentId { get; set; }
            public IList<AnswerData> PageInfo { get; set; }
        };
    }

}
