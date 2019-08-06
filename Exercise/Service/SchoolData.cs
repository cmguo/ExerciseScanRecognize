using Exercise.Algorithm;
using Exercise.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
        private static readonly string[] gradeNames = { null,
            "小一", "小二", "小三", "小四", "小五", "小六",
            "初一", "初二", "初三",
            "高一", "高二", "高三",
        };

        public string ClassId { get; set; }
        public int Grade { get; set; }
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

        public IList<bool> EmptyPages
        {
            get => AnswerPages == null ? null 
                : AnswerPages.Select(p => p == Page.EmptyPage).ToList();
            set => AnswerPages = value == null ? null : value.Select(p => p ? Page.EmptyPage : null).ToList();
        }

        [JsonIgnore]
        public double PageCount => AnswerPages == null ? double.NaN : 
            AnswerPages.Sum(p => (p == null || p == Page.EmptyPage) ? 0 : p.DuplexPageCount);

        [JsonIgnore]
        public double Score => AnswerPages == null ? 0 : 
            AnswerPages.Sum(p => p == null ? 0 : p.DuplexScore);

        [JsonIgnore]
        public IList<AnswerData> Answers => GetAnswers();

        public override string ToString()
        {
            return StudentNo + " " + Name;
        }

        private IList<AnswerData> GetAnswers()
        {
            List<AnswerData> answers = new List<AnswerData>();
            foreach (Page p in AnswerPages)
            {
                if (p == null)
                    continue;
                if (p.Answer != null)
                {
                    p.Answer.ImageName = p.PageName;
                    p.Answer.PageId = p.PageIndex;
                    answers.Add(p.Answer);
                }
                if (p.Another != null && p.Another.Answer != null)
                {
                    p.Another.Answer.ImageName = p.Another.PageName;
                    p.Another.Answer.PageId = p.Another.PageIndex;
                    answers.Add(p.Another.Answer);
                }
            }
            return answers;
        }

    }
}
