using Base.Misc;
using Base.Mvvm;
using Base.Service;
using Exercise.Service;
using MyToolkit.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalBase.Model;

namespace Exercise.Model
{
    public class ExerciseModel : ModelBase
    {
        private static ExerciseModel s_instance;
        public static ExerciseModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ExerciseModel();
                }
                return s_instance;
            }
        }

        public enum ExceptionType : int
        {
            None = 0,
            NoPageCode, 
            PageCodeMissMatch, 
            NoStudentCode, // StudentCodeMissMatch,
            AnalyzeException,
            AnswerException,
            CorrectionException,
            PageLost,
        }

        public enum ResolveType : int
        {
            Ignore, 
            RemovePage, 
            RemoveStudent,
            RemoveDuplexPage
        }

        public class Exception
        {
            public ExceptionType Type { get; set; }
            public Page Page { get; set; }
        }

        public class ExceptionList
        {
            public ExceptionType Type { get; set; }
            public ObservableCollection<Exception> Exceptions { get; set; }

            public ExceptionList()
            {
                Exceptions = new ObservableCollection<Exception>();
            }
        }

        private static readonly Page sEmptyPage = new Page();

        public string SavePath { get; private set; }
        public ObservableCollection<ExceptionList> Exceptions { get; private set; }
        public ObservableCollection<Page> PageDropped { get; private set; }
        public ExerciseData ExerciseData { get; private set; }
        public ObservableCollection<StudentInfo> PageStudents { get; private set; }

        private SchoolModel schoolModel = SchoolModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;
        private SubmitModel submitModel = SubmitModel.Instance;
        private HistoryModel historyModel = HistoryModel.Instance;
        private IExercise service;

        private string exerciseId;

        public ExerciseModel()
        {
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            scanModel.PropertyChanged += ScanModel_PropertyChanged;
            service = Services.Get<IExercise>();
            Exceptions = new ObservableCollection<ExceptionList>();
            PageDropped = new ObservableCollection<Page>();
            PageStudents = new ObservableCollection<StudentInfo>();
            /* Test
            ExerciseData = new ExerciseData() { Title = "三角函数" };
            AddException(ExceptionType.NoPageCode, new Page());
            AddException(ExceptionType.NoStudentCode, new Page());
            AddException(ExceptionType.PageCodeMissMatch, new Page());
            StudentInfo student = new StudentInfo() { Name = "张三", TalNo = "1234" };
            AddException(ExceptionType.AnalyzeException, new Page() { Student = student });
            AddException(ExceptionType.AnswerException, new Page() { Student = student });
            AddException(ExceptionType.CorrectionException, new Page() { Student = student });
            */
        }

        internal void Discard()
        {
            throw new NotImplementedException();
        }

        public async Task NewTask()
        {
            string path = historyModel.NewSavePath();
            scanModel.SetSavePath(path);
            SavePath = path;
            await schoolModel.Refresh();
        }

        public async Task MakeResult()
        {
            schoolModel.GetLostPageStudents(s => {
                s.AnswerPages = new List<Page>();
                for (int i = 0; i < ExerciseData.Pages.Count; i += 2)
                    s.AnswerPages.Add(new Page() { PageIndex = i, Student = s });
                PageStudents.Add(s);
            });
            foreach (Page p in PageStudents.Select(s => s.AnswerPages.Where(p => p.StudentCode == null)))
            {
                AddException(ExceptionType.PageLost, p);
            }
            await Save();
        }

        public async Task SubmitResult()
        {
            await Save();
            string path = SavePath;
            await submitModel.Save(path, exerciseId, ExerciseData, PageStudents);
            BackgroudWork.Execute(() => submitModel.Submit(path));
        }

        public async Task Save()
        {
            await schoolModel.Save(SavePath);
            await scanModel.Save();
            await JsonPersistent.Save(SavePath + "\\exercise.json", ExerciseData);
            await historyModel.Save(SavePath);
        }

        public async Task Load(string path)
        {
            Clear();
            await schoolModel.Load(path);
            await scanModel.Load(path);
            ExerciseData = await JsonPersistent.Load<ExerciseData>(path + "\\exercise.json");
            SavePath = path;
        }

        public void Clear()
        {
            exerciseId = null;
            ExerciseData = null;
            Exceptions.Clear();
            PageDropped.Clear();
            schoolModel.Clear();
            scanModel.Clear();
            SavePath = null;
        }

        public void Resolve(Exception ex, ResolveType type)
        {
            if (type == ResolveType.Ignore)
            {
                if (ex.Type == ExceptionType.AnswerException)
                    ex.Page.AnswerExceptions.Clear();
                else if (ex.Type == ExceptionType.CorrectionException)
                    ex.Page.CorrectionExceptions.Clear();
                RemoveException(ex.Type, ex.Page);
                return;
            }
            RemovePage(ex.Page, type);
        }

        private async void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCode")
            {
                if (scanModel.PageCode == null)
                    return;
                exerciseId = scanModel.PageCode;
                ExerciseData = await service.GetExercise(exerciseId);
                RaisePropertyChanged("ExerciseData");
                scanModel.SetExerciseData(ExerciseData);
            }
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Page page in e.NewItems)
                    AddPage(page);
            }
        }

        private void AddPage(Page page)
        {
            ExceptionType type = ExceptionType.None;
            if (page.PaperCode == null)
                type = ExceptionType.NoPageCode;
            else if (page.PaperCode != scanModel.PageCode)
                type = ExceptionType.PageCodeMissMatch;
            else if (page.StudentCode == null || (page.Student = schoolModel.GetStudent(page.StudentCode)) == null)
                type = ExceptionType.NoStudentCode;
            if (type != ExceptionType.None)
            {
                AddException(type, page);
                return;
            }
            page.Another.Student = page.Student; // 可能单面，就是自己
            if (page.Student.AnswerPages == null)
            {
                page.Student.AnswerPages = new List<Page>();
                for (int i = 0; i < ExerciseData.Pages.Count; i += 2)
                    page.Student.AnswerPages.Add(new Page() { PageIndex = i, Student = page.Student });
                PageStudents.Add(page.Student);
            }
            int pageIndex = page.PageIndex / 2;
            Page old = page.Student.AnswerPages[pageIndex];
            page.Student.AnswerPages[pageIndex] = page;
            old.Student = null;
            RemovePage(old, ResolveType.RemovePage);
            if (page.Answer == null)
            {
                AddException(ExceptionType.AnalyzeException, page);
            }
            else
            {
                if (page.AnswerExceptions.Count > 0)
                    AddException(ExceptionType.AnswerException, page);
                if (page.CorrectionExceptions.Count > 0)
                    AddException(ExceptionType.CorrectionException, page);
                if (page.Another != page)
                {
                    if (page.Another.AnswerExceptions.Count > 0)
                        AddException(ExceptionType.AnswerException, page.Another);
                    if (page.Another.CorrectionExceptions.Count > 0)
                        AddException(ExceptionType.CorrectionException, page.Another);
                }
            }
        }

        public void RemovePage(Page page, ResolveType type)
        {
            // 如果 Student 为 Null，肯定是 type = DuplexPage
            if (page.Student == null)
            {
                RemovePage(page);
                return;
            }
            if (type == ResolveType.RemoveStudent)
            {
                for (int i = 0; i < page.Student.AnswerPages.Count; ++i)
                {
                    if (page.Student.AnswerPages[i] != null)
                    {
                        RemovePage(page.Student.AnswerPages[i]);
                    }
                }
                page.Student.AnswerPages = null;
                PageStudents.Remove(page.Student);
                return;
            }
            int pageIndex = page.PageIndex / 2;
            if (page.Student.AnswerPages[pageIndex] == page)
            {
                page.Student.AnswerPages[pageIndex] = sEmptyPage;
                if (type != ResolveType.RemoveDuplexPage && page.Another != page)
                {
                    page.Student.AnswerPages[pageIndex] = page.Another;
                    page.Another.Another = page.Another;
                    page.Another = page;
                }
            }
            else if (page.Student.AnswerPages[pageIndex].Another == page)
            {
                page.Student.AnswerPages[pageIndex].Another = page.Student.AnswerPages[pageIndex];
                page.Another = page;
            }
            RemovePage(page);
        }

        public void RemovePage(Page page)
        {
            RemoveException(ExceptionType.None, page);
            page.Student = null;
            PageDropped.Add(page);
            scanModel.ReleasePage(page);
            if (page.Another != page)
            {
                RemoveException(ExceptionType.None, page.Another);
                page.Another.Student = null;
                PageDropped.Add(page.Another);
                scanModel.ReleasePage(page.Another);
                page.Another.Another = page.Another;
            }
            page.Another = page;
        }

        public void UpdatePage(ExceptionType type, Page page)
        {
            RemoveException(type, page);
            if (type == ExceptionType.NoStudentCode)
            {
                AddPage(page);
            }
        }

        private void AddException(ExceptionType type, Page page)
        {
            Exception item = new Exception() { Type = type, Page = page };
            if (type == ExceptionType.AnalyzeException || type == ExceptionType.PageCodeMissMatch)
                type = ExceptionType.NoPageCode;
            ExceptionList list = Exceptions.FirstOrDefault(e => type.CompareTo(e.Type) <= 0);
            if (list == null)
            {
                list = new ExceptionList() { Type = type };
                Exceptions.Add(list);
            }
            else if (list.Type != type)
            {
                int index = Exceptions.IndexOf(list);
                list = new ExceptionList() { Type = type };
                Exceptions.Insert(index, list);
            }
            list.Exceptions.Add(item);
        }

        private void RemoveException(ExceptionType type, Page page)
        {
            if (type == ExceptionType.None)
            {
                foreach (ExceptionType t in Enum.GetValues(typeof(ExceptionType)))
                    if (t != type && t != ExceptionType.PageLost)
                        RemoveException(t, page);
                return;
            }
            if (type == ExceptionType.AnalyzeException)
                type = ExceptionType.NoPageCode;
            ExceptionList list = Exceptions.FirstOrDefault(e => type.CompareTo(e.Type) == 0);
            if (list == null) return;
            Exception ex = list.Exceptions.FirstOrDefault(e => e.Page == page);
            if (ex == null) return;
            list.Exceptions.Remove(ex);
            if (list.Exceptions.Count == 0)
                Exceptions.Remove(list);
        }

    }
}
