using Exercise.Algorithm;
using System;
using System.Collections.Generic;

namespace Exercise.Service
{
    public class UseLog
    {
        //public long Date { get; set; }
        public long UserId { get; set; }
        public string SchoolName { get; set; }
        public string Course { get; set; }
        public int PageCount { get; set; }
        public int StudentCount { get; set; }
        public long ScanDuration { get; set; }
        public int ExceptionCount { get; set; }
        public long ResolveDuration { get; set; }
        public long SubmitDuration { get; set; }
    }

}
