using Account.Model;
using Base.Misc;
using Base.Mvvm;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Exercise.Service.HistoryData;

namespace Exercise.Model
{

    public class HistoryModel : NotifyBase
    {
        private static readonly Logger Log = Logger.GetLogger<HistoryModel>();

        private static HistoryModel s_instance;
        public static HistoryModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new HistoryModel();
                }
                return s_instance;
            }
        }

        public enum DurationType
        {
            None,
            Scan,
            Summary,
            Resolve,
            Submit
        }

        public ObservableCollection<Record> LocalRecords { get; private set; }

        public Record WorkingRecord { get; private set; }

        public IList<Record> Records { get; private set; }

        public int PageCount { get; private set; }

        private static readonly int PAGE_SIZE = 10;

        private IExercise service;
        private DurationType durType;
        private long durStart;

        public HistoryModel()
        {
            LocalRecords = new ObservableCollection<Record>();
            service = Services.Get<IExercise>();
        }

        public Record NewRecord()
        {
            string path = Component.DATA_PATH + "\\" + DateTime.Now.ToString("yyyyMMdd")
                + "\\" + DateTime.Now.ToString("T").Replace(':', '.');
            Directory.CreateDirectory(path);
            WorkingRecord = new Record()
            {
                LocalPath = path,
                ScanDate = DateTime.Now.Timestamp(),
                UseLog = new UseLog()
                {
                    UserId = AccountModel.Instance.Account.Id, 
                    SchoolName = AccountModel.Instance.Account.SchoolName
                }
            };
            LocalRecords.Add(WorkingRecord);
            return WorkingRecord;
        }

        public void SetTitle(string title, string course)
        {
            WorkingRecord.Name = title;
            WorkingRecord.UseLog.Course = course;
        }

        public void BeginDuration(DurationType type)
        {
            if (durType != DurationType.None)
                EndDuration();
            durType = type;
            durStart = DateTime.Now.Timestamp();
        }

        public void EndDuration(int count = 0)
        {
            if (durType == DurationType.None || WorkingRecord.UseLog == null)
                return;
            long end = DateTime.Now.Timestamp();
            long dur = end - durStart;
            switch (durType)
            {
                case DurationType.Scan:
                    WorkingRecord.UseLog.PageCount = count;
                    WorkingRecord.UseLog.ScanDuration += dur;
                    break;
                case DurationType.Summary:
                    WorkingRecord.UseLog.StudentCount = count;
                    break;
                case DurationType.Resolve:
                    WorkingRecord.UseLog.ExceptionCount += count;
                    WorkingRecord.UseLog.ResolveDuration += dur;
                    break;
                case DurationType.Submit:
                    WorkingRecord.UseLog.SubmitDuration += dur;
                    break;
            }
            durType = DurationType.None;
        }

        public async Task Save()
        {
            WorkingRecord.ScanDate = DateTime.Now.Timestamp();
            await JsonPersistent.SaveAsync(WorkingRecord.LocalPath + "\\record.json", WorkingRecord);
        }

        public void Clear()
        {
            Remove(WorkingRecord);
            WorkingRecord = null;
        }

        public void Remove(Record record)
        {
            service.SubmitUseLog(WorkingRecord.UseLog);
            LocalRecords.Remove(record);
            Directory.Delete(record.LocalPath, true);
        }

        public async Task<Record> Load()
        {
            LocalRecords.Clear();
            await LoadLocal();
            WorkingRecord = LocalRecords.FirstOrDefault();
            return WorkingRecord;
        }

        private async Task LoadLocal()
        {
            string path = Component.DATA_PATH;
            foreach (string path1 in Directory.EnumerateDirectories(path))
            {
                foreach (string path2 in Directory.EnumerateDirectories(path1))
                {
                    string path3 = path2 + "\\record.json";
                    if (!File.Exists(path3))
                    {
                        try
                        {
                            Directory.Delete(path2, true);
                        }
                        catch
                        {
                        }
                        continue;
                    }
                    try
                    {
                        Record record = await JsonPersistent.LoadAsync<Record>(path3);
                        record.LocalPath = path2;
                        LocalRecords.Add(record);
                    }
                    catch (Exception e)
                    {
                        Log.w("LoadLocal", e.Message);
                    }
                }
                try
                {
                    Directory.Delete(path1);
                }
                catch
                {
                }
            }
        }

        private static readonly IList<Record> empty = new List<Record>();

        public async Task LoadPage(int page)
        {
            HistoryData records = await service.getRecords(new HistoryData.Range() { Page = page + 1, Size = PAGE_SIZE });
            int pageCount = 0;
            if (records != null)
            {
                pageCount = (records.TotalCount + PAGE_SIZE - 1) / PAGE_SIZE;
                Records = records.SubmitRecordList;
            }
            else
            {
                Records = null;
            }
            RaisePropertyChanged("Records");
            if (pageCount != PageCount)
            {
                PageCount = pageCount;
                RaisePropertyChanged("PageCount");
            }
        }

        public async Task ModifyRecord(Record record, string old)
        {
            try
            {
                await service.updateRecord(new Record() { HomeworkId = record.HomeworkId, Name = record.Name });
            }
            catch
            {
                record.Name = old;
                throw;
            }
        }

    }
}
