using Exercise.Algorithm;
using System;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class SubmitData
    {
        public String exerciseId { get; private set; }
        public List<AnswerInfo> studentAnswers { get; private set; }

        public class AnswerInfo
        {
            public String studentId { get; private set; }
            public String paperId { get; private set; }
            public List<AnswerData> pages { get; private set; }
        };
    }

}
