using Exercise.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Service
{
    public class SchoolData
    {
        public string SchoolName { get; set; }
        public StudentInfo[] StudentInfoList { get; set; }
        public ClassInfo[] ClassInfoList { get; set; }
    }

    public class ClassInfo
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }

        [JsonIgnore]
        public IList<StudentInfo> Students { get; set; }
    }

    public class StudentInfo
    {
        public string Id { get; set; }
        public string ClassId { get; set; }
        public string Name { get; set; }
        public string StudentNo { get; set; }
        public string TalNo { get; set; }

        [JsonIgnore]
        public IList<Page> AnswerPages { get; set; }

        public override string ToString()
        {
            return TalNo + " " + Name;
        }

    }
}
