using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TalBase.Model;
using static Exercise.Algorithm.AnswerData;
using static Exercise.Model.ExerciseModel;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.Model
{
    public class PageAnalyze : NotifyBase
    {

        private const string NULL_ANSWER = "未作答";

        [JsonIgnore]
        public IList<QuestionException> AnswerExceptions { get; set; }

        [JsonIgnore]
        public IList<QuestionException> CorrectionExceptions { get; set; }

        public class QuestionException : ModelBase
        {
            private bool _HasException;
            public bool HasException
            {
                get => _HasException;
                set { _HasException = value; RaisePropertyChanged("HasException"); }
            }

            public IList<string> Answers => Question.ItemInfo[0].Value.Split(',')
                .Concat(new string[] { NULL_ANSWER }).ToList();
            
            private string _SelectedAnswer;
            public string SelectedAnswer
            {
                get => _SelectedAnswer;
                set
                {
                    _SelectedAnswer = value;
                    RaisePropertyChanged("SelectedAnswer");
                }
            }

            public PageData.Question Question { get; }
            public AnswerData.Question Answer { get; }

            public QuestionException(PageData.Question q, AnswerData.Question a)
            {
                Question = q;
                Answer = a;
                HasException = true;
            }

            public void Select(bool score)
            {
                string answer = String.Join(",", Answer.ItemInfo
                    .Where(i => i.AnalyzeResult != null)
                    .SelectMany(i => i.AnalyzeResult.Select(r => r.Value))
                    .Where(v => v != null));
                if (!score)
                {
                    if (answer.Length == 0)
                        answer = NULL_ANSWER;
                }
                SelectedAnswer = answer;
            }

            public void InputNumber(int? n)
            {
                if (n != null)
                {
                    SelectedAnswer += n.ToString();
                }
                else if (SelectedAnswer.Length > 0)
                {
                    SelectedAnswer.Remove(0, 1);
                }
            }

            public bool Confirm(bool score)
            {
                if (score)
                {
                    float total = float.Parse(Question.ItemInfo[0].TotalScore);
                    if (float.Parse(SelectedAnswer) > total)
                        return false;
                }
                Answer.ItemInfo.All(i =>
                {
                    i.StatusOfItem = -1;
                    i.AnalyzeResult = new List<Result>();
                    if (SelectedAnswer != NULL_ANSWER && SelectedAnswer != "")
                    {
                        i.AnalyzeResult.Add(new Result() { Value = SelectedAnswer });
                    }
                    return true;
                });
                HasException = false;
                return true;
            }
        }

        private IList<QuestionException> _Questions;
        public IList<QuestionException> Questions
        {
            get => _Questions;
            set
            {
                _Questions = value;
                RaisePropertyChanged("Questions");
            }
        }

        private QuestionException _SelectedQuestion;
        public QuestionException SelectedQuestion
        {
            get => _SelectedQuestion;
            set
            {
                _SelectedQuestion = value;
                if (_SelectedQuestion != null)
                    _SelectedQuestion.Select(Questions == CorrectionExceptions);
                RaisePropertyChanged("SelectedQuestion");
            }
        }

        public static PageAnalyze Analyze(Page page)
        {
            IList<QuestionException> AnswerExceptions = SelectExceptions(page.MetaData, page.Answer, AreaType.SingleChoice);
            IList<QuestionException> CorrectionExceptions = SelectExceptions(page.MetaData, page.Answer, AreaType.Answer);
            if (AnswerExceptions == null && CorrectionExceptions == null)
                return null;
            return new PageAnalyze() { AnswerExceptions = AnswerExceptions, CorrectionExceptions = CorrectionExceptions };
        }

        public void Switch(ExceptionType type)
        {
            Questions = type == ExceptionType.AnswerException ? AnswerExceptions : CorrectionExceptions;
            SelectedQuestion = Questions[0];
        }

        public bool Confirm()
        {
            return SelectedQuestion.Confirm(Questions == CorrectionExceptions);
        }

        public bool Next()
        {
            int n = Questions.IndexOf(SelectedQuestion);
            if (++n < Questions.Count)
            {
                SelectedQuestion = Questions[n];
                return true;
            }
            return false;
        }

        private static List<QuestionException> SelectExceptions(PageData data, AnswerData answer, AreaType type)
        {
            List<QuestionException> exceptions = answer.AreaInfo.Where(a => a.AreaType == type)
                .SelectMany(a => a.QuestionInfo.Where(q => q.ItemInfo.Any(i => i.StatusOfItem > 0)))
                .Select(q => new QuestionException(GetQuestion(data, q.QuestionId), q)).ToList();
            if (exceptions.Count == 0)
                exceptions = null;
            return exceptions;
        }

        private static PageData.Question GetQuestion(PageData data, string questionId)
        {
            return data.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == questionId).First();
        }

        public void ClearException(ExceptionType type)
        {
            if (type == ExceptionType.AnswerException)
            {
                Clear(AnswerExceptions);
                AnswerExceptions = null;
            }
            else if (type == ExceptionType.CorrectionException)
            {
                Clear(CorrectionExceptions);
                CorrectionExceptions = null;
            }
        }

        private void Clear(IList<QuestionException> Exceptions)
        {
            Exceptions.All(q =>
            {
                q.Answer.ItemInfo.All(i =>
                {
                    if (i.StatusOfItem != 0)
                        i.StatusOfItem = -2;
                    i.AnalyzeResult = null;
                    return true;
                });
                return true;
            });
        }
    }
}
