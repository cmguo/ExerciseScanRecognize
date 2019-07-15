using Exercise.Algorithm;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TalBase.Model;
using static Exercise.Model.ExerciseModel;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.Model
{
    public class Page
    {
        public int TotalIndex { get; set; }
        public int ScanBatch { get; set; }
        public int ScanIndex { get; set; }

        [JsonIgnore]
        public byte[] PageData { get; set; } // 算法会调整
        [JsonIgnore]
        public String PagePath { get; set; }
        public String PageName { get; set; }

        [JsonConverter(typeof(ExceptionJsonConverter))]
        public System.Exception Exception { get; set; }
        public String PaperCode { get; set; }
        public int PageIndex { get; set; } // 0, 2, 4
        public String StudentCode { get; set; }
        public AnswerData Answer { get; set; }

        [JsonIgnore]
        public PageData MetaData { get; set; }
        public Page Another { get; set; }

        [JsonIgnore]
        public StudentInfo Student { get; set; }

        [JsonIgnore]
        public IList<QuestionException> AnswerExceptions { get; set; }

        [JsonIgnore]
        public IList<QuestionException> CorrectionExceptions { get; set; }

        public void CalcException()
        {
            AnswerExceptions = SelectExceptions(AreaType.SingleChoice);
            CorrectionExceptions = SelectExceptions(AreaType.Answer);
        }

        private List<QuestionException> SelectExceptions(AreaType type)
        {
            List<QuestionException> exceptions = Answer.AreaInfo.Where(a => a.AreaType == type)
                .SelectMany(a => a.QuestionInfo.Where(q => q.ItemInfo.Any(i => i.StatusOfItem > 0)))
                .Select(q => new QuestionException(GetQuestion(q.QuestionId), q)).ToList();
            if (exceptions.Count == 0)
                exceptions = null;
            return exceptions;
        }

        private PageData.Question GetQuestion(string questionId)
        {
            return MetaData.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == questionId).First();
        }

        public void ClearException(ExceptionType type)
        {
            if (type == ExceptionType.AnswerException)
            {
                AnswerExceptions.All(q =>
                {
                    q.Answer.ItemInfo.All(i =>
                    {
                        if (i.StatusOfItem > 0)
                            i.StatusOfItem = -2;
                        return true;
                    });
                    return true;
                });
                AnswerExceptions = null;
            }
            else if (type == ExceptionType.CorrectionException)
            {
                CorrectionExceptions.All(q =>
                {
                    q.Answer.ItemInfo.All(i =>
                    {
                        if (i.StatusOfItem > 0)
                            i.StatusOfItem = -2;
                        return true;
                    });
                    return true;
                });
                CorrectionExceptions = null;
            }
        }

        public class QuestionException : ModelBase
        {
            private bool _HasException;
            public bool HasException
            {
                get => _HasException;
                set { _HasException = value; RaisePropertyChanged("HasException"); }
            }

            public PageData.Question Question { get; }
            public AnswerData.Question Answer { get; }

            public QuestionException(PageData.Question q, AnswerData.Question a)
            {
                Question = q;
                Answer = a;
                HasException = true;
            }
        }

        public void Swap(Page o)
        {
            int i = TotalIndex;
            TotalIndex = o.TotalIndex;
            o.TotalIndex = i;
            string s = PageName;
            PageName = o.PageName;
            o.PageName = s;
            s = PagePath;
            PagePath = o.PagePath;
            o.PagePath = s;
            System.Exception e = Exception;
            Exception = o.Exception;
            o.Exception = e;
            AnswerData a = Answer;
            Answer = o.Answer;
            o.Answer = a;
        }

    }
}
