using Exercise.Algorithm;
using Exercise.Model;
using Newtonsoft.Json;
using System;
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

    public class ClassInfo : IComparable<ClassInfo>
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

        public int CompareTo(ClassInfo other)
        {
            return Compare(ClassName, other.ClassName);
        }

        private static readonly string hans = "一二三四五六七八九十初高";

        public static int Compare(string x, string y)
        {
            int i = 0;
            for (; i < x.Length && i < y.Length; ++i)
            {
                if (Char.IsDigit(x[i]) && Char.IsDigit(y[i]))
                {
                    int ix = i + 1;
                    while (ix < x.Length && Char.IsDigit(x[ix]))
                        ++ix;
                    int nx = Int32.Parse(x.Substring(i, ix - i));
                    int iy = i + 1;
                    while (iy < y.Length && Char.IsDigit(y[iy]))
                        ++iy;
                    int ny = Int32.Parse(y.Substring(i, iy - i));
                    if (nx == ny)
                    {
                        i = ix - 1;
                        continue;
                    }
                    return nx - ny;
                }
                else if (x[i] == y[i])
                {
                    continue;
                }
                else if (hans.Contains(x[i]) && hans.Contains(y[i]))
                {
                    return hans.IndexOf(x[i]) - hans.IndexOf(y[i]);
                }
                else
                {
                    return x[i] - y[i];
                }
            }
            return x.Length - y.Length;
        }

    }

    public class StudentInfo : IComparable<StudentInfo>
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

        public int CompareTo(StudentInfo other)
        {
            return ToString().CompareTo(other.ToString());
        }

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
