using Exercise.Algorithm;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class ExerciseData
    {
        public string ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public IList<PageData> Pages { get; set; }
    }
}
