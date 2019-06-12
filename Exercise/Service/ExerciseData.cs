using Exercise.Algorithm;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class ExerciseData
    {
        public string exerciseId { get; set; }
        public IList<PageData> pages { get; set; }
    }
}
