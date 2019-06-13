using Exercise.Algorithm;
using System;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class SubmitData
    {
        public String homeworkId { get; private set; } // 批次记录ID，后端生成
        public String paperId { get; private set; }
        public IList<AnswerInfo> data { get; private set; }

        public class AnswerInfo
        {
            public String studentId { get; private set; }
            public IList<AnswerData> pages { get; private set; }
        };
    }

}
