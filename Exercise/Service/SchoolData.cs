﻿using Exercise.Model;
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

        public IList<bool> EmptyPages
        {
            get => AnswerPages == null ? null 
                : AnswerPages.Select(p => p == Page.EmptyPage).ToList();
            set => AnswerPages = value == null ? null : value.Select(p => p ? Page.EmptyPage : null).ToList();
        }
        public double Score => AnswerPages == null ? 0 : AnswerPages.Sum(p => p == null ? 0 : p.DuplexScore);

        public override string ToString()
        {
            return StudentNo + " " + Name;
        }

    }
}
