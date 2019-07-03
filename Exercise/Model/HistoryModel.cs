using Base.Misc;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalBase.Model;
using static Exercise.Service.HistoryData;

namespace Exercise.Model
{

    public class HistoryModel : ModelBase
    {
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

        public ObservableCollection<Record> Records { get; private set; }

        private IExercise service;

        private int page = 0;

        public HistoryModel()
        {
            LocalRecords = new ObservableCollection<Record>();
            Records = new ObservableCollection<Record>();
            service = Services.Get<IExercise>();
        }

        public string NewSavePath()
        {
            string path = System.Environment.CurrentDirectory + "\\扫描试卷\\" 
                + DateTime.Now.ToString("D") + "\\" + DateTime.Now.ToString("T").Replace(':', '.');
            Directory.CreateDirectory(path);
            return path;
        }

        public async Task Save(string path)
        {
            List<ClassDetail> classes = SchoolModel.Instance.Classes.Select(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                ResultCount = c.Students.Where(s => s.AnswerPages != null && s.AnswerPages.Any(p => p.StudentCode != null)).Count(),
            }).ToList();
            Record record = new Record() { ExerciseName = ExerciseModel.Instance.ExerciseData.Title, ClassDetails = classes };
            await JsonPersistent.Save(path + "\\record.json", record);
        }

        public void Remove(Record record)
        {
            LocalRecords.Remove(record);
            Directory.Delete(record.LocalPath, true);
        }

        public async Task Load()
        {
            LocalRecords.Clear();
            Records.Clear();
            await LoadLocal();
            await LoadMore();
        }

        private async Task LoadLocal()
        {
            string path = System.Environment.CurrentDirectory + "\\扫描试卷";
            foreach (string path1 in Directory.EnumerateDirectories(path))
            {
                foreach (string path2 in Directory.EnumerateDirectories(path1))
                {
                    string path3 = path2 + "\\record.json";
                    if (!File.Exists(path3))
                    {
                        Directory.Delete(path2, true);
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
                        Debug.WriteLine("LoadLocal", e.Message);
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

        public async Task LoadMore()
        {
            HistoryData records = await service.getRecords(page);
            ++page;
            foreach (Record r in records.Records)
                Records.Add(r);
        }

    }
}
