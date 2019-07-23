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

        public ObservableCollection<Record> LocalRecords { get; private set; }

        public IList<Record> Records { get; private set; }

        public int PageCount { get; private set; }

        private static readonly int PAGE_SIZE = 10;

        private IExercise service;

        public HistoryModel()
        {
            LocalRecords = new ObservableCollection<Record>();
            service = Services.Get<IExercise>();
        }

        public string NewSavePath()
        {
            string path = Component.DATA_PATH + "\\" + DateTime.Now.ToString("yyyyMMdd")
                + "\\" + DateTime.Now.ToString("T").Replace(':', '.');
            Directory.CreateDirectory(path);
            return path;
        }

        public async Task Save(string path)
        {
            if (LocalRecords.Any(r => r.LocalPath == path))
                return;
            Record record = new Record() { Name = ExerciseModel.Instance.ExerciseData.Title, ScanDate = DateTime.Now.Timestamp() };
            await JsonPersistent.Save(path + "\\record.json", record);
        }

        public void Remove(string path)
        {
            Record record = LocalRecords.Where(r => r.LocalPath == path).FirstOrDefault();
            if (record != null)
                LocalRecords.Remove(record);
            Directory.Delete(path, true);
        }

        public void Remove(Record record)
        {
            LocalRecords.Remove(record);
            Directory.Delete(record.LocalPath, true);
        }

        public async Task Load()
        {
            LocalRecords.Clear();
            await LoadLocal();
            await LoadPage(0);
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
                        Record record = await JsonPersistent.Load<Record>(path3);
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
