using Exercise.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Service
{

    public class HistoryData
    {
        public int TotalCount { get; set; }
        public int Start { get; set; }
        public List<Record> Records { get; set; }


        public class ClassDetail
        {
            public string ClassName { get; set; }
            public int ResultCount { get; set; }
        }

        public class Record
        {
            [JsonIgnore]
            public string LocalPath { get; set; }
            public string ExerciseName { get; set; }
            public string DateTime { get; set; }
            public IList<ClassDetail> ClassDetails { get; set; }
        }
    }

}
