using Exercise.Algorithm;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class ExerciseData
    {
        public string Title { get; set; }
        [JsonProperty("typesettingResultList")]
        public IList<PageData> Pages { get; set; }
    }
}
