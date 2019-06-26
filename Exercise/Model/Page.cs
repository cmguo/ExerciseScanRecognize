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
        public int ScanBatch { get; set; }
        public int ScanIndex { get; set; }
        public byte[] PageData { get; set; } // 算法会调整
        public String PagePath { get; set; }
        public String Md5Name { get; set; }

        public Exception Exception { get; set; }
        public String PaperCode { get; set; }
        public int PageIndex { get; set; } // 0, 2, 4
        public String StudentCode { get; set; }
        public AnswerData Answer { get; set; }

        public Page Another { get; set; }

        [JsonIgnore]
        public StudentInfo Student { get; set; }

        [JsonIgnore]
        public IList<AnswerData.Question> AnswerExceptions { get; set; }

        [JsonIgnore]
        public IList<AnswerData.Question> CorrectionExceptions { get; set; }
    }
}
