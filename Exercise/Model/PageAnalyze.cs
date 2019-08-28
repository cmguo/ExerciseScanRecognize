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

        private static Dictionary<QuestionType, IList<QuestionType>> questionTypeMap = 
            new Dictionary<QuestionType, IList<QuestionType>>();
        private static IList<PageData.Question> normalizedQuestions;
        private static IDictionary<string, IList<string>> standardAnswers;

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

        public static void SetExerciseData(ExerciseData data)
        {
            if (data == null)
            {
                questionTypeMap = null;
                normalizedQuestions = null;
                standardAnswers = null;
            }
            else
            {
                questionTypeMap = data.QuestionTypeMap;
                normalizedQuestions = data.Questions;
                standardAnswers = data.Answers;
            }
        }

        public static PageAnalyze Analyze(Page page, bool apply)
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
                PageData.Area ap = page.MetaData.AreaInfo.Where(a => a.AreaId == aa.AreaId).First();
                if (aa.AreaLocation == null)
                    aa.AreaLocation = areaLocation;
                if (apply)
                    aa.ApplyFrom(ap);
                IList<ItemException> exceptions = type == AreaType.Answer ? CorrectionExceptions : AnswerExceptions;
                foreach (AnswerData.Question qa in aa.QuestionInfo)
                {
                    PageData.Question qp = normalizedQuestions.Where(q => q.QuestionId == qa.QuestionId).First();
                    if (apply)
                        qa.ApplyFrom(qp);
                    IList<string> qe = null;
                    if (standardAnswers != null)
                        standardAnswers.TryGetValue(qa.QuestionId, out qe);
                    for (int i = 0; i < qa.ItemInfo.Count; ++i)
                    {
                        AnswerData.Item ia = qa.ItemInfo[i];
                        PageData.Item ip = qp.ItemInfo[ia.Index];
                        if (apply)
                            ia.ApplyFrom(ip);
                        if (ia.PagingInfo != PagingInfo.None && ia.PagingInfo != PagingInfo.Down)
                            continue;
                        string ie = (qe == null || ia.Index >= qe.Count) ? null : qe[ia.Index]; // 注意分页
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

        public static PageAnalyze Analyze(Page page, Page another, bool apply)
        {
            PageAnalyze analyze = Analyze(page, apply);
            if (analyze == null)
                return another.Analyze == null ? null : new PageAnalyze() { Another = another.Analyze };
            if (another.Analyze == null)
                another.Analyze = new PageAnalyze() { Another = analyze };
            else
                another.Analyze.Another = analyze;
            analyze.Another = another.Analyze;
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
            PageData.Item ip, AnswerData.Item ia, string ie)
        {
            double score = 0;
            string answer = ia.AnalyzeResult == null ? ""
                : string.Join("", ia.AnalyzeResult.Where(r => r != null).Select(r => r.Value).Where(v => v != null));
            int status = 0;
            if (type == AreaType.Choice || type == AreaType.Judge)
            {
                if (ie != null)
                {
                    ie = ie.Replace('对', 'T');
                    ie = ie.Replace('错', 'F');
                    ie = ie.Replace('✓', 'T');
                    ie = ie.Replace('×', 'F');
                    ie = ie.Replace("正确", "T");
                    ie = ie.Replace("错误", "F");
                    ie = String.Join("", ie.Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))).ToUpper();
                }
                qtype = MapQuestionType(qtype);
                if (qtype == QuestionType.SingleChoice || qtype == QuestionType.Judge)
                {
                    if (ia.AnalyzeResult.Count > 1)
                    {
                        status = 100;
                    }
                    else if (answer == ie)
                    {
                        score = ip.TotalScore;
                    }
                }
                else if (qtype == QuestionType.MultipleChoice)
                {
                    if (answer == ie)
                    {
                        score = ip.TotalScore;
                    }
                    else if (answer.Length > 0 && answer.Except(ie).Count() == 0)
                    {
                        if (ie.Except(answer).Count() == 0)
                            score = ip.TotalScore;
                        else
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
                    if (ia.AnalyzeResult.Last() == null || ia.AnalyzeResult.Last().Value == null)
                    {
                        ia.StatusOfItem = 101;
                        answer += "0";
                    }
                    score = float.Parse(answer);
                    if (score > ip.TotalScore)
                    {
                        ia.StatusOfItem = 102;
                        score = ip.TotalScore;
                    }
                }
            }
            if (ia.StatusOfItem == 0)
            {
                ia.StatusOfItem = status;
            }
            ia.Score = score;
            return answer;
        }

        private static QuestionType MapQuestionType(QuestionType qtype)
        {
            return questionTypeMap == null ? qtype 
                : questionTypeMap.Where(d => d.Key == qtype || d.Value.Contains(qtype))
                    .Select(d => d.Key).FirstOrDefault();
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
            public string SAnswer { get; }
            public Location Location { get; }

            public ItemException(AnswerData.Area a, PageData.Question q, PageData.Item p, AnswerData.Item i, 
                string e, string w)
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
                    if (score < 0 || score > Problem.TotalScore)
                        throw new System.Exception("输入值不在有效范围中");
                    if (score != Math.Floor(score) && !Answers.Contains(SelectedAnswer))
                        throw new System.Exception("输入值不在有效范围中");
                    Answer.Score = score;
                }
                else if (SelectedAnswer == SAnswer)
                {
                    Answer.Score = Problem.TotalScore;
                }
                else if (QuestionType == QuestionType.MultipleChoice)
                {
                    Answer.Score = (SelectedAnswer.Except(SAnswer).Count() == 0) ? Problem.HalfScore : 0;
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
