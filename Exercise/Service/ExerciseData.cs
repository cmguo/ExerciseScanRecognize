using Exercise.Algorithm;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Exercise.Service
{
    public class ExerciseData
    {
        public string Title { get; set; }
        public int SubjectCode { get; set; }
        public string SubjectName { get; set; }

        [JsonProperty("typesettingResultList")]
        public IList<PageData> Pages { get; set; }
        [JsonProperty("questionDetailItems")]
        public IDictionary<string, IList<string>> Answers { get; set; }
        public Dictionary<QuestionType, IList<QuestionType>> QuestionTypeMap { get; set; }

        [JsonIgnore]
        public IList<PageData.Question> Questions { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Questions = new List<PageData.Question>();
            PageData.Question ql = null;
            foreach (PageData.Question q in Pages.SelectMany(p => p.AreaInfo.SelectMany(a => a.QuestionInfo)))
            {
                if (ql != null && q.Index == ql.Index)
                {
                    PageData.Item il = ql.ItemInfo.Last();
                    IList<PageData.Item> itemInfo = ql.ItemInfo.Union(q.ItemInfo.SkipWhile(i => i.Index <= il.Index)).ToList();
                    ql = new PageData.Question() { Index = ql.Index, QuestionType = ql.QuestionType,
                        QuestionId = ql.QuestionId, ItemInfo = itemInfo };
                    Questions[Questions.Count - 1] = ql;
                }
                else
                {
                    Questions.Add(q);
                    ql = q;
                }
            }
        }

    }
}
