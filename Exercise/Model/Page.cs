﻿using Exercise.Algorithm;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Exercise.Model
{
    public class Page
    {

        public static readonly Page EmptyPage = new Page();

        public int TotalIndex { get; set; }
        public int ScanBatch { get; set; }
        public int ScanIndex { get; set; }

        [JsonIgnore]
        [Obsolete("PageData is deprecated")]
        public byte[] PageData { get; set; } // 算法会调整
        [JsonIgnore]
        public String PagePath { get; set; }
        public String PageName { get; set; }

        [JsonConverter(typeof(ExceptionJsonConverter))]
        public System.Exception Exception { get; set; }
        public String PaperCode { get; set; }
        public int PageIndex { get; set; } // 0, 2, 4
        public String StudentCode { get; set; }
        public AnswerData Answer { get; set; }

        [JsonIgnore]
        public PageData MetaData { get; set; }
        [JsonIgnore]
        public IList<ExerciseData.Question> StandardAnswers { get; set; }

        public Page Another { get; set; }

        [JsonIgnore]
        public int PageIndexPlusOne => PageIndex + 1;

        [JsonIgnore]
        public StudentInfo Student { get; set; }

        [JsonIgnore]
        public PageAnalyze Analyze { get; set; }

        [JsonIgnore]
        public double Score => Analyze == null ? 0 : Analyze.Score;

        [JsonIgnore]
        public double DuplexScore => Score + (Another == null ? 0 : Another.Score);

        [JsonIgnore]
        public double StudentScore => CalcStudentScore();

        public void Swap(Page o)
        {
            int i = TotalIndex;
            TotalIndex = o.TotalIndex;
            o.TotalIndex = i;
            string s = PageName;
            PageName = o.PageName;
            o.PageName = s;
            s = PagePath;
            PagePath = o.PagePath;
            o.PagePath = s;
            Exception e = Exception;
            Exception = o.Exception;
            o.Exception = e;
            AnswerData a = Answer;
            Answer = o.Answer;
            o.Answer = a;
        }

        private double CalcStudentScore()
        {
            if (Student == null)
                return double.NaN;
            double s = Student.Score;
            if (Student.AnswerPages == null || Student.AnswerPages[PageIndex / 2] == null)
                s += DuplexScore;
            return s;
        }

    }
}
