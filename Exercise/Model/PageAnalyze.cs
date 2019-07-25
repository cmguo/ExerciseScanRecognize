﻿using Base.Mvvm;
using Exercise.Algorithm;
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

        [JsonIgnore]
        public IList<ItemException> AnswerExceptions { get; set; }

        [JsonIgnore]
        public IList<ItemException> CorrectionExceptions { get; set; }

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

            public IList<string> Answers => Problem.Value.Split(',')
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

            public PageData.Item Problem { get; }
            public AnswerData.Item Answer { get; }

            public ItemException(AnswerData.Area a, PageData.Question q, PageData.Item p, AnswerData.Item i)
            {
                AreaType = a.AreaType;
                Name = (q.Index + 1).ToString();
                if (q.ItemInfo.Count > 1)
                {
                    Name += ".";
                    Name += p.Index;
                }
                Problem = p;
                Answer = i;
                HasException = true;
            }

            public void Select(bool score)
            {
                string answer = Answer.AnalyzeResult == null ? "" 
                    : String.Join("", Answer.AnalyzeResult.Select(r => r.Value).Where(v => v != null));
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
                    SelectedAnswer = SelectedAnswer.Remove(SelectedAnswer.Length - 1, 1);
                }
            }

            public bool Confirm(bool score)
            {
                if (score)
                {
                    try
                    {
                        float total = float.Parse(Problem.TotalScore);
                        if (float.Parse(SelectedAnswer) > total)
                            return false;
                    }
                    catch
                    {
                        return false;
                    }
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
        }

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
                if (_SelectedException != null)
                    _SelectedException.Select(Exceptions == CorrectionExceptions);
                RaisePropertyChanged("SelectedException");
            }
        }

        public static PageAnalyze Analyze(Page page)
        {
            IList<ItemException> AnswerExceptions = SelectExceptions(page.MetaData, page.Answer, AreaType.SingleChoice, AreaType.MultiChoice);
            IList<ItemException> CorrectionExceptions = SelectExceptions(page.MetaData, page.Answer, AreaType.Answer);
            if (AnswerExceptions == null && CorrectionExceptions == null)
                return null;
            return new PageAnalyze() { AnswerExceptions = AnswerExceptions, CorrectionExceptions = CorrectionExceptions };
        }

        public void Switch(ExceptionType type)
        {
            Exceptions = type == ExceptionType.AnswerException ? AnswerExceptions : CorrectionExceptions;
            SelectedException = Exceptions[0];
        }

        public bool Confirm()
        {
            return SelectedException.Confirm(Exceptions == CorrectionExceptions);
        }

        public bool Next()
        {
            int n = Exceptions.IndexOf(SelectedException);
            SelectedException = Exceptions.Skip(n + 1).Where(e => e.HasException).FirstOrDefault();
            if (SelectedException == null)
                SelectedException = Exceptions.Take(n).Where(e => e.HasException).FirstOrDefault();
            return SelectedException != null;
        }

        private static List<ItemException> SelectExceptions(PageData data, AnswerData answer, params AreaType[] types)
        {
            List<ItemException> exceptions = new List<ItemException>();
            foreach (AnswerData.Area a in answer.AreaInfo)
            {
                if (!types.Contains(a.AreaType))
                    continue;
                foreach (AnswerData.Question qa in a.QuestionInfo)
                {
                    PageData.Question qp = GetQuestion(data, qa.QuestionId);
                    for (int i = 0; i < qp.ItemInfo.Count; ++i)
                    {
                        AnswerData.Item item = qa.ItemInfo[i];
                        if (item.StatusOfItem > 0)
                        {
                            exceptions.Add(new ItemException(a, qp, qp.ItemInfo[i], item));
                            continue;
                        }
                        if (item.StatusOfItem < 0)
                            continue;
                        if (a.AreaType == AreaType.SingleChoice)
                        {
                            if (item.AnalyzeResult.Count > 1)
                            {
                                item.StatusOfItem = 100;
                                exceptions.Add(new ItemException(a, qp, qp.ItemInfo[i], item));
                            }
                        }
                        else if (a.AreaType == AreaType.Answer)
                        {
                            string score = string.Join(",", item.AnalyzeResult
                                .Where(r => r != null)
                                .Select(r => r.Value)
                                .Where(v => v != null));
                            float total = float.Parse(qp.ItemInfo[i].TotalScore);
                            if (score.Length > 0 && float.Parse(score) > total)
                            {
                                item.StatusOfItem = 100;
                                exceptions.Add(new ItemException(a, qp, qp.ItemInfo[i], item));
                            }
                        }
                    }
                }
            }
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

        private void Clear(IList<ItemException> Exceptions)
        {
            foreach (ItemException i in Exceptions)
            {
                if (i.Answer.StatusOfItem != 0)
                    i.Answer.StatusOfItem = -2;
                i.Answer.AnalyzeResult = null;
            };
        }
    }
}
