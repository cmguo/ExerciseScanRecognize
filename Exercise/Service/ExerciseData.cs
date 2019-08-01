using Exercise.Algorithm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class ExerciseData
    {
        public string Title { get; set; }
        [JsonProperty("typesettingResultList")]
        public IList<PageData> Pages { get; set; }
        public IList<Question> Answers { get; set; }

        public class Item
        {
            public string Value { get; set; }
        }

        public class Question
        {
            public string QuestionId { get; set; }
            public IList<Item> ItemInfo { get; set; }
        }

    }
}
