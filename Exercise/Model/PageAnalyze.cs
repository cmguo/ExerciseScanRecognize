using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using TalBase.Model;
using static Exercise.Model.ExerciseModel;

namespace Exercise.Model
{
    public class PageAnalyze : NotifyBase
    {

        public const string NULL_ANSWER = "未作答";
        private static readonly ExerciseData.Item EmptyItem = new ExerciseData.Item();

        private static Dictionary<QuestionType, IList<QuestionType>> questionTypeMap = 
            new Dictionary<QuestionType, IList<QuestionType>>();
        private static IList<ExerciseData.Question> standardAnswers = new List<ExerciseData.Question>();

        #region Properties

        public IList<ItemException> AnswerExceptions { get; set; }

        public IList<ItemException> CorrectionExceptions { get; set; }

        public double Score { get; private set; }

        public double DuplexScore => Score + (Another == null ? 0 : Another.Score);

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

        public PageAnalyze Another { get; private set; }

        #endregion

        #region Public static methods

        public static void SetQuestionTypeMap(Dictionary<QuestionType, IList<QuestionType>> map)
        {
            questionTypeMap = map;
        }

        public static void SetStandardAnswers(IList<ExerciseData.Question> answers)
        {
            standardAnswers = answers;
        }

        public static PageAnalyze Analyze(Page page)
        {
            if (page.Answer == null)
                return null;
            double score = 0;
            IList<ItemException> AnswerExceptions = new List<ItemException>();
            IList<ItemException> CorrectionExceptions = new List<ItemException>();
            Location areaLocation = page.Answer.AreaLocation;
            foreach (AnswerData.Area aa in page.Answer.AreaInfo)
            {
                AreaType type = aa.AreaType;
                if (aa.AreaLocation == null)
                    aa.AreaLocation = areaLocation;
                IList<ItemException> exceptions = type == AreaType.Answer ? CorrectionExceptions : AnswerExceptions;
                foreach (AnswerData.Question qa in aa.QuestionInfo)
                {
                    PageData.Question qp = GetQuestion(page.MetaData, qa.QuestionId);
                    ExerciseData.Question qe = standardAnswers == null ? null
                        : standardAnswers.Where(q => q.QuestionId == qa.QuestionId).FirstOrDefault();
                    for (int i = 0; i < qa.ItemInfo.Count; ++i)
                    {
                        PageData.Item ip = qp.ItemInfo[i];
                        if (ip.PagingInfo != PagingInfo.None && ip.PagingInfo != PagingInfo.Down)
                            continue;
                        AnswerData.Item ia = qa.ItemInfo[i];
                        ExerciseData.Item ie = qe == null ? EmptyItem : qe.ItemInfo[ip.Index]; // 注意分页
                        string answer = Analyze(type, qp.QuestionType, ip, ia, ie);
                        if (ia.StatusOfItem > 0)
                            exceptions.Add(new ItemException(aa, qp, ip, ia, ie, answer));
                        score += ia.Score;
                    }
                }
            }
            if (AnswerExceptions.Count == 0)
                AnswerExceptions = null;
            if (CorrectionExceptions.Count == 0)
                CorrectionExceptions = null;
            return new PageAnalyze() { AnswerExceptions = AnswerExceptions, CorrectionExceptions = CorrectionExceptions, Score = score };
        }

        public static PageAnalyze Analyze(Page page, PageAnalyze another)
        {
            PageAnalyze analyze = Analyze(page);
            if (analyze == null)
                return another == null ? null : new PageAnalyze() { Another = another };
            another.Another = analyze;
            analyze.Another = another;
            return analyze;
        }

        public static IEnumerable<double> GetScoreDetail(IEnumerable<PageData.Question> questions, IEnumerable<AnswerData> answerss)
        {
            IEnumerable<AnswerData.Question> answers = answerss.SelectMany(a => a.AreaInfo.SelectMany(a1 => a1.QuestionInfo));
            IList<double> scores = new List<double>(questions.SelectMany(q => Enumerable.Repeat(double.NaN, q.ItemInfo.Count)));
            int qindex = 0;
            foreach (var pq in questions)
            {
                int index = 0;
                AnswerData.Question aq = answers.FirstOrDefault();
                while (aq != null && aq.QuestionId == pq.QuestionId)
                {
                    foreach (AnswerData.Item ai in aq.ItemInfo.SkipWhile(i => i.Index < index))
                    {
                        scores[qindex + ai.Index] = ai.Score;
                    }
                    if (aq.ItemInfo.Count > 0)
                        index = aq.ItemInfo.Last().Index + 1;
                    answers = answers.Skip(1);
                    aq = answers.FirstOrDefault();
                    continue;
                }
                qindex += pq.ItemInfo.Count;
            }
            return scores;
        }

        #endregion

        #region Public methods

        public void Switch(ExceptionType type)
        {
            Exceptions = type == ExceptionType.AnswerException ? AnswerExceptions : CorrectionExceptions;
            SelectedException = Exceptions[0];
        }

        public void Confirm()
        {
            Score -= SelectedException.Answer.Score;
            try
            {
                SelectedException.Confirm();
            }
            finally
            {
                Score += SelectedException.Answer.Score;
            }
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

        #endregion

        private void Clear(IList<ItemException> Exceptions)
        {
            foreach (ItemException i in Exceptions)
            {
                Score -= i.Answer.Score;
                i.Clear();
            };
        }

        #region Private static methods

        private static string Analyze(AreaType type, QuestionType qtype, 
            PageData.Item ip, AnswerData.Item ia, ExerciseData.Item ie)
        {
            double score = 0;
            string answer = ia.AnalyzeResult == null ? ""
                : string.Join("", ia.AnalyzeResult.Where(r => r != null).Select(r => r.Value).Where(v => v != null));
            if (ia.StatusOfItem == 0)
            {
                if (type == AreaType.Choice)
                {
                    qtype = MapQuestionType(qtype);
                    if (qtype == QuestionType.SingleChoice || qtype == QuestionType.Judge)
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
                    else if (qtype == QuestionType.MultipleChoice)
                    {
                        if (answer == ie.Value)
                        {
                            score = ip.TotalScore;
                        }
                        else if (answer.Length > 0 && answer.Except(ie.Value).Count() == 0)
                        {
                            score = ip.HalfScore;
                        }
                    }
                }
                else if (type == AreaType.FillBlank)
                {
                    if (answer == "T")
                        score = ip.TotalScore;
                }
                else if (type == AreaType.Answer)
                {
                    if (ia.AnalyzeResult == null || ia.AnalyzeResult.Count == 0)
                    {
                        ia.StatusOfItem = 100;
                    }
                    else
                    {
                        if (ia.AnalyzeResult.Last() == null || ia.AnalyzeResult.Last().Value == "")
                        {
                            ia.StatusOfItem = 101;
                            answer += "0";
                        }
                        score = float.Parse(answer);
                        if (score > ip.TotalScore)
                        {
                            ia.StatusOfItem = 102;
                            score = 0;
                        }
                    }
                }
            }
            ia.Score = score;
            return answer;
        }

        private static QuestionType MapQuestionType(QuestionType qtype)
        {
            return questionTypeMap.Where(d => d.Key == qtype || d.Value.Contains(qtype))
                .Select(d => d.Key).FirstOrDefault();
        }

        private static PageData.Question GetQuestion(PageData data, string questionId)
        {
            return data.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == questionId).First();
        }

        #endregion

        public class ItemException : ModelBase
        {
            public AreaType AreaType;
            public QuestionType QuestionType;
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
            public PageData.Item Problem { get; }
            public AnswerData.Item Answer { get; }
            public ExerciseData.Item SAnswer { get; }
            public Location Location { get; }

            public ItemException(AnswerData.Area a, PageData.Question q, PageData.Item p, AnswerData.Item i, 
                ExerciseData.Item e, string w)
            {
                AreaType = a.AreaType;
                QuestionType = MapQuestionType(q.QuestionType);
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
                Answers = Problem.Value.Split(',').Concat(new string[] { NULL_ANSWER }).ToList();
                SelectedAnswer = w;
                Location = new Location()
                {
                    LeftTop = new Point() { X = a.AreaLocation.LeftTop.X, Y = i.ItemLocation.LeftTop.Y },
                    RightBottom = new Point() { X = a.AreaLocation.RightBottom.X, Y = i.ItemLocation.RightBottom.Y },
                };
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

            public void Confirm()
            {
                if (AreaType == AreaType.Answer)
                {
                    if (SelectedAnswer == "")
                        throw new System.Exception("请为本题打分");
                    double score = double.Parse(SelectedAnswer);
                    if (score > Problem.TotalScore)
                        throw new System.Exception("输入值不在有效范围中");
                    Answer.Score = score;
                }
                else if (SelectedAnswer == SAnswer.Value)
                {
                    Answer.Score = Problem.TotalScore;
                }
                else if (QuestionType == QuestionType.MultipleChoice)
                {
                    Answer.Score = (SelectedAnswer.Except(SAnswer.Value).Count() == 0) ? Problem.HalfScore : 0;
                }
                else
                {
                    Answer.Score = 0;
                }

                Answer.StatusOfItem = -1;
                Answer.AnalyzeResult = new List<AnswerData.Result>();
                if (SelectedAnswer != NULL_ANSWER && SelectedAnswer != "")
                {
                    Answer.AnalyzeResult.Add(new AnswerData.Result() { Value = SelectedAnswer });
                }
                HasException = false;
            }

            public void Clear()
            {
                if (Answer.StatusOfItem != 0)
                    Answer.StatusOfItem = -2;
                Answer.AnalyzeResult = null;
                Answer.Score = 0;
            }
        }

    }
}
