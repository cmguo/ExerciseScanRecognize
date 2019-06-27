using Base.Misc;
using Base.Mvvm;
using Base.Service;
using Exercise.Service;
using MyToolkit.Command;
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
            //RemoveDuplexPage
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
        private List<Page> emptyPages;

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

        public void Discard()
        {
            string path = SavePath;
            Clear();
            if (path != null)
                Directory.Delete(path, true);
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
            if (ExerciseData == null)
                throw new NullReferenceException("没有有效试卷信息");
            schoolModel.GetLostPageStudents(s => {
                s.AnswerPages = new List<Page>(emptyPages);
                PageStudents.Add(s);
            });
            foreach (StudentInfo s in PageStudents)
            {
                for (int i = 0; i < s.AnswerPages.Count; ++i)
                {
                    if (s.AnswerPages[i] == null)
                    {
                        Page p = new Page() { PageIndex = i * 2, Student = s };
                        s.AnswerPages[i] = p;
                        AddException(ExceptionType.PageLost, p);
                    }
                }
            }
            await Save();
        }

        public async Task SubmitResult()
        {
            await Save();
            string path = SavePath;
            await submitModel.Save(path, exerciseId, schoolModel.Classes, PageStudents);
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
            emptyPages = null;
            ExerciseData = null;
            PageStudents.Clear();
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
                    ex.Page.AnswerExceptions = null;
                else if (ex.Type == ExceptionType.CorrectionException)
                    ex.Page.CorrectionExceptions = null;
                RemoveException(ex.Type, ex.Page);
                return;
            }
            RemovePage(ex.Page, type);
        }

        public void Resolve(ExceptionList el, ResolveType type)
        {
            foreach (Exception ex in el.Exceptions)
                Resolve(ex, type);
        }

        private void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCode")
            {
                if (scanModel.PageCode == null)
                    return;
                LoadExercise();
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

        private async void LoadExercise()
        {
            try
            {
                exerciseId = scanModel.PageCode;
                ExerciseData = await service.GetExercise(exerciseId);
                int n = (ExerciseData.Pages.Count + 1) / 2;
                emptyPages = new List<Page>(n);
                while (n-- > 0)
                    emptyPages.Add(null);
                RaisePropertyChanged("ExerciseData");
                scanModel.SetExerciseData(ExerciseData);
            }
            catch
            {
                ExerciseData = null;
                RaisePropertyChanged("ExerciseData");
            }
        }

        private void AddPage(Page page)
        {
            ExceptionType type = ExceptionType.None;
            if (page.PaperCode == null)
                type = ExceptionType.NoPageCode;
            else if (page.PaperCode != scanModel.PageCode)
                type = ExceptionType.PageCodeMissMatch;
            else if (page.StudentCode == null 
                || (page.Student = schoolModel.GetStudent(page.StudentCode)) == null)
                type = ExceptionType.NoStudentCode;
            if (type != ExceptionType.None)
            {
                AddException(type, page);
                return;
            }
            if (page.Another != null)
                page.Another.Student = page.Student; // 可能单面，就是自己
            if (page.Student.AnswerPages == null)
            {
                page.Student.AnswerPages = new List<Page>(emptyPages);
                PageStudents.Add(page.Student);
            }
            int pageIndex = page.PageIndex / 2;
            Page old = page.Student.AnswerPages[pageIndex];
            page.Student.AnswerPages[pageIndex] = page;
            if (old != null)
            {
                old.Student = null;
                RemovePage(old, ResolveType.RemovePage);
            }
            if (page.Answer == null)
            {
                AddException(ExceptionType.AnalyzeException, page);
            }
            else
            {
                if (page.AnswerExceptions != null)
                    AddException(ExceptionType.AnswerException, page);
                if (page.CorrectionExceptions != null)
                    AddException(ExceptionType.CorrectionException, page);
                if (page.Another != null)
                {
                    if (page.Another.AnswerExceptions != null)
                        AddException(ExceptionType.AnswerException, page.Another);
                    if (page.Another.CorrectionExceptions != null)
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
                page.Student.AnswerPages[pageIndex] = null;
                if (/*type != ResolveType.RemoveDuplexPage && */page.Another != null)
                {
                    page.Student.AnswerPages[pageIndex] = page.Another;
                    page.Another = null;
                }
            }
            else if (page.Student.AnswerPages[pageIndex].Another == page)
            {
                page.Student.AnswerPages[pageIndex].Another = null;
            }
            RemovePage(page);
        }

        public void RemovePage(Page page)
        {
            RemoveException(ExceptionType.None, page);
            page.Student = null;
            PageDropped.Add(page);
            if (page.PagePath != null)
            {
                scanModel.ReleasePage(page);
            }
            if (page.Another != null)
            {
                RemoveException(ExceptionType.None, page.Another);
                page.Another.Student = null;
                PageDropped.Add(page.Another);
                scanModel.ReleasePage(page.Another);
                page.Another = null;
            }
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
