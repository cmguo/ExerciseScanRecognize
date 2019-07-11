using Exercise.Algorithm;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Exercise.Service.SubmitData;

namespace Exercise.Model
{
    public class Page
    {
        public int TotalIndex { get; set; }
        public int ScanBatch { get; set; }
        public int ScanIndex { get; set; }

        [JsonIgnore]
        public byte[] PageData { get; set; } // 算法会调整
        [JsonIgnore]
        public String PagePath { get; set; }
        public String PageName { get; set; }

        [JsonConverter(typeof(ExceptionJsonConverter))]
        public Exception Exception { get; set; }
        public String PaperCode { get; set; }
        public int PageIndex { get; set; } // 0, 2, 4
        public String StudentCode { get; set; }
        public AnswerData Answer { get; set; }

        [JsonIgnore]
        public PageData MetaData { get; set; }
        public Page Another { get; set; }

        [JsonIgnore]
        public StudentInfo Student { get; set; }

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

    }
}
