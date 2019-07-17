using Base.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TalBase.Model;

namespace Exercise.Service
{

    public partial class HistoryData
    {
        public long TotalCount { get; set; }
        public Record[] SubmitRecordList { get; set; }


        public partial class Range
        {
            public int Page { get; set; }
            public int Size { get; set; }
        }

        public partial class Record : ModelBase
        {
            public int HomeworkId { get; set; }
            private string _Name;

            public string Name
            {
                get => _Name;
                set { _Name = value; RaisePropertyChanged("Name"); }
            }
            public long ScanDate { get; set; }
            [JsonIgnore]
            public DateTime DataTime => SystemUtil.DateTimeFromTimestamp(ScanDate);
            [JsonIgnore]
            public string LocalPath { get; internal set; }
            public IList<ClassDetail> DetailList { get; set; }
        }

        public partial class ClassDetail
        {
            public string Name { get; set; }
            public string ClassId { get; set; }
            public IList<StudentDetail> SubmitStudentList{ get; set; }
            public IList<StudentDetail> LostStudentList { get; set; }
        }

        public partial class StudentDetail
        {
            public string Name { get; set; }
            public string StudentNo { get; set; }
        }

    }

}
