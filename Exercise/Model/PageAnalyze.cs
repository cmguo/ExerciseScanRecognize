using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TalBase.Model;
using static Exercise.Model.ExerciseModel;

namespace Exercise.Model
{
    public class PageAnalyze : NotifyBase
    {

        private const string NULL_ANSWER = "未作答";
        private static readonly ExerciseData.Item EmptyItem = new ExerciseData.Item();

        public IList<ItemException> AnswerExceptions { get; set; }

        public IList<ItemException> CorrectionExceptions { get; set; }

        public double Score { get; private set; }

        private IList<ItemException> _Exceptions;
        public IList<ItemException> Exceptions
        {
            get => _Exceptions;
            set
            {
                _Exceptions = value;
                RaisePropertyChanged("Exceptions");
            }
        }

        private ItemException _SelectedException;
        public ItemException SelectedException
        {
            get => _SelectedException;
            set
            {
                _SelectedException = value;
                RaisePropertyChanged("SelectedException");
            }
        }

        public static PageAnalyze Analyze(Page page)
        {
            if (page.Answer == null)
                return null;
            double score = 0;
            IList<ItemException> AnswerExceptions = new List<ItemException>();
            IList<ItemException> CorrectionExceptions = new List<ItemException>();
            foreach (AnswerData.Area aa in page.Answer.AreaInfo)
            {
                AreaType type = aa.AreaType;
                IList<ItemException> exceptions = type == AreaType.Answer ? CorrectionExceptions : AnswerExceptions;
                foreach (AnswerData.Question qa in aa.QuestionInfo)
                {
                    PageData.Question qp = GetQuestion(page.MetaData, qa.QuestionId);
                    ExerciseData.Question qe = page.StandardAnswers == null ? null
                        : page.StandardAnswers.Where(q => q.QuestionId == qa.QuestionId).FirstOrDefault();
                    for (int i = 0; i < qa.ItemInfo.Count; ++i)
                    {
                        PageData.Item ip = qp.ItemInfo[i];
                        AnswerData.Item ia = qa.ItemInfo[i];
                        ExerciseData.Item ie = qe == null ? EmptyItem : qe.ItemInfo[i];
                        string answer;
                        double s = Analyze(type, ip, ia, ie, out answer);
                        if (ia.StatusOfItem != 0)
                            exceptions.Add(new ItemException(aa, qp, ip, ia, ie, s, answer));
                        score += s;
                    }
                }
            }
            if (AnswerExceptions.Count == 0)
                AnswerExceptions = null;
            if (CorrectionExceptions.Count == 0)
                CorrectionExceptions = null;
            return new PageAnalyze() { AnswerExceptions = AnswerExceptions, CorrectionExceptions = CorrectionExceptions, Score = score };
        }

        public void Switch(ExceptionType type)
        {
            Exceptions = type == ExceptionType.AnswerException ? AnswerExceptions : CorrectionExceptions;
            SelectedException = Exceptions[0];
        }

        public bool Confirm()
        {
            Score -= SelectedException.Score;
            bool result = SelectedException.Confirm();
            Score += SelectedException.Score;
            return result;
        }

        public bool Next()
        {
            int n = Exceptions.IndexOf(SelectedException);
            SelectedException = Exceptions.Skip(n + 1).Where(e => e.HasException).FirstOrDefault();
            if (SelectedException == null)
                SelectedException = Exceptions.Take(n).Where(e => e.HasException).FirstOrDefault();
            return SelectedException != null;
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

        private void Clear(IList<ItemException> Exceptions)
        {
            foreach (ItemException i in Exceptions)
            {
                Score -= i.Score;
                i.Clear();
            };
        }

        private static double Analyze(AreaType type, PageData.Item ip, AnswerData.Item ia, ExerciseData.Item ie, out string answer)
        {
            double score = 0;
             answer = ia.AnalyzeResult == null ? ""
                : string.Join(",", ia.AnalyzeResult.Where(r => r != null).Select(r => r.Value).Where(v => v != null));
            if (ia.StatusOfItem == 0)
            {
                if (type == AreaType.SingleChoice)
                {
                    if (ia.AnalyzeResult.Count > 1)
                    {
                        ia.StatusOfItem = 100;
                    }
                    else if (answer == ie.Value)
                    {
                        score = ip.TotalScore;
                    }
                }
                else if (type == AreaType.MultiChoice)
                {
                    if (answer == ie.Value)
                    {
                        score = ip.TotalScore;
                    }
                }
                else if (type == AreaType.Judge)
                {
                    if (answer == ie.Value)
                    {
                        score = ip.TotalScore;
                    }
                }
                else if (type == AreaType.FillBlank)
                {
                    if (ia.AnalyzeResult != null && ia.AnalyzeResult.Count > 0)
                        score = ip.TotalScore;
                }
                else if (type == AreaType.Answer)
                {
                    if (answer.Length > 0)
                        score = float.Parse(answer);
                    if (score > ip.TotalScore)
                    {
                        ia.StatusOfItem = 100;
                    }
                }
            }
            return score;
        }

        private static PageData.Question GetQuestion(PageData data, string questionId)
        {
            return data.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == questionId).First();
        }

        public class ItemException : ModelBase
        {
            public AreaType AreaType;
            public string Name { get; private set; }

            private bool _HasException;
            public bool HasException
            {
                get => _HasException;
                set { _HasException = value; RaisePropertyChanged("HasException"); }
            }

            public IList<string> Answers { get; }

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

            public double Score { get; private set; }

            public PageData.Item Problem { get; }
            public AnswerData.Item Answer { get; }
            public ExerciseData.Item SAnswer { get; }
            public Location Location { get; }

            public ItemException(AnswerData.Area a, PageData.Question q, PageData.Item p, AnswerData.Item i, 
                ExerciseData.Item e, double score, string w)
            {
                AreaType = a.AreaType;
                Name = (q.Index + 1).ToString();
                HasException = true;
                if (q.ItemInfo.Count > 1)
                {
                    Name += ".";
                    Name += p.Index + 1;
                }
                Problem = p;
                Answer = i;
                SAnswer = e;
                Score = score;
                Answers = Problem.Value.Split(',').Concat(new string[] { NULL_ANSWER }).ToList();
                SelectedAnswer = w;
                //Location = new Location()
                //{
                //    LeftTop = new Point() { X = a.AreaLocation.LeftTop.X, Y = i.ItemLocation.LeftTop.Y },
                //    RightBottom = new Point() { X = a.AreaLocation.RightBottom.X, Y = i.ItemLocation.RightBottom.Y },
                //};
                Location = i.ItemLocation;
            }

            public void InputNumber(int? n)
            {
                if (n != null)
                {
                    SelectedAnswer += n.ToString();
                }
                else if (SelectedAnswer.Length > 0)
                {
                    SelectedAnswer = SelectedAnswer.Remove(SelectedAnswer.Length - 1, 1);
                }
            }

            public bool Confirm()
            {
                if (AreaType == AreaType.Answer)
                {
                    try
                    {
                        double score = double.Parse(SelectedAnswer);
                        if (score > Problem.TotalScore)
                            return false;
                        Score = score;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else if (SelectedAnswer == SAnswer.Value)
                {
                    Score = Problem.TotalScore;
                }

                Answer.StatusOfItem = -1;
                Answer.AnalyzeResult = new List<AnswerData.Result>();
                if (SelectedAnswer != NULL_ANSWER && SelectedAnswer != "")
                {
                    Answer.AnalyzeResult.Add(new AnswerData.Result() { Value = SelectedAnswer });
                }
                HasException = false;
                return true;
            }

            public void Clear()
            {
                if (Answer.StatusOfItem != 0)
                    Answer.StatusOfItem = -2;
                Answer.AnalyzeResult = null;
                Score = 0;
            }
        }

    }
}
