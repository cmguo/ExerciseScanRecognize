using Base.Misc;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalBase.Model;

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

        public ObservableCollection<Record> Records { get; private set; }

        private IExercise service;

        private int page = 0;

        public HistoryModel()
        {
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
                ResultCount = c.Students.Where(s => s.AnswerPages.Any(p => p.StudentCode == null)).Count(),
            }).ToList();
            Record record = new Record() { ExerciseName = ExerciseModel.Instance.ExerciseData.Title, ClassDetails = classes };
            await JsonPersistent.Save(path + "\\record.json", record);
        }

        public async Task Load()
        {
            Records.Clear();
            await LoadLocal();
            await LoadMore();
        }

        private async Task LoadLocal()
        {
            string path = System.Environment.CurrentDirectory + "\\扫描试卷";
            foreach (string path2 in Directory.EnumerateDirectories(path))
            {
                string path1 = path2 + "\\record.json";
                if (!File.Exists(path1))
                    continue;
                try
                {
                    Record record = await JsonPersistent.Load<Record>(path1);
                    Records.Add(record);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("LoadLocal", e.Message);
                }
            }
        }

        public async Task LoadMore()
        {
            RecordData records = await service.getRecords(page);
            ++page;
            foreach (Record r in records.Records)
                Records.Add(r);
        }

    }
}
