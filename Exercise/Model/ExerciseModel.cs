using Base.Misc;
using Base.Mvvm;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
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
            AnalyzeException,
            NoStudentCode, // StudentCodeMissMatch,
            PageLost,
            AnswerException,
            CorrectionException,
        }

        public enum ResolveType : int
        {
            Ignore,
            RemovePage,
            RemoveStudent,
            Resolve
            //RemoveDuplexPage
        }

        public enum RemoveType : int
        {
            SinglePage,
            DuplexPage,
            Student,
        }

        public class Exception
        {
            public ExceptionType Type { get; set; }
            public int Index { get; set; }
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

        public ExerciseData ExerciseData { get; private set; }
        public ObservableCollection<StudentInfo> PageStudents { get; private set; }

        private SchoolModel schoolModel = SchoolModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;
        private SubmitModel submitModel = SubmitModel.Instance;
        private HistoryModel historyModel = HistoryModel.Instance;
        private IExercise service;

        private List<Page> emptyPages;
        private Exception targetException;

        public ExerciseModel()
        {
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            scanModel.PropertyChanged += ScanModel_PropertyChanged;
            service = Services.Get<IExercise>();
            Exceptions = new ObservableCollection<ExceptionList>();
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
            //*/
        }

        public void Discard()
        {
            string path = SavePath;
            Clear();
            if (path != null)
                historyModel.Remove(path);
        }

        public async Task NewTask()
        {
            string path = historyModel.NewSavePath();
            scanModel.SetSavePath(path);
            SavePath = path;
            await schoolModel.Refresh();
        }

        internal void FillAll()
        {
            Page l = PageStudents.First().AnswerPages.Where(p => p != null).First();
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
                        s.AnswerPages[i] = l;
                    }
                }
            }
            Exceptions.Clear();
        }

        public async Task MakeResult()
        {
            if (ExerciseData == null)
                throw new NullReferenceException("没有有效试卷信息");
            foreach (StudentInfo s in PageStudents)
            {
                AddException(s);
            }
            await Save();
        }

        public async Task SubmitResult()
        {
            await Save();
            string path = SavePath;
            await submitModel.Save(path, scanModel.PageCode, schoolModel.Classes, PageStudents);
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
            ExerciseData = await JsonPersistent.Load<ExerciseData>(path + "\\exercise.json");
            int n = (ExerciseData.Pages.Count + 1) / 2;
            emptyPages = new List<Page>(n);
            while (n-- > 0)
                emptyPages.Add(null);
            scanModel.SetExerciseData(ExerciseData);
            await scanModel.Load(path);
            SavePath = path;
            foreach (StudentInfo s in PageStudents)
            {
                AddException(s);
            }
        }

        public void Clear()
        {
            emptyPages = null;
            targetException = null;
            ExerciseData = null;
            PageStudents.Clear();
            Exceptions.Clear();
            schoolModel.Clear();
            scanModel.Clear();
            SavePath = null;
        }

        public Task ScanOne(Exception ex)
        {
            lock (Exceptions)
            {
                targetException = ex;
                scanModel.Scan(1);
            }
            return Task.Run(() =>
            {
                lock (Exceptions)
                {
                    while (targetException != null)
                    {
                        Monitor.Wait(Exceptions);
                    }
                }
            });
        }

        public async Task CancelScanOne()
        {
            await scanModel.CancelScan();
            lock (Exceptions)
            {
                targetException = null;
                Monitor.PulseAll(Exceptions);
            }
        }

        public void Resolve(Exception ex, ResolveType type)
        {
            Page oldPage = ex.Page;
            if (type == ResolveType.Ignore
                || type == ResolveType.Resolve)
            {
                if (ex.Type == ExceptionType.AnswerException)
                {
                    oldPage.Answer.AnswerExceptions.All(q =>
                    {
                        q.ItemInfo.All(i =>
                        {
                            if (i.StatusOfItem > 0)
                                i.StatusOfItem = -2;
                            return true;
                        });
                        return true;
                    });
                    oldPage.Answer.AnswerExceptions = null;
                }
                else if (ex.Type == ExceptionType.CorrectionException)
                {
                    oldPage.Answer.CorrectionExceptions.All(q =>
                    {
                        q.ItemInfo.All(i =>
                        {
                            if (i.StatusOfItem > 0)
                                i.StatusOfItem = -2;
                            return true;
                        });
                        return true;
                    });
                    oldPage.Answer.CorrectionExceptions = null;
                }
                RemoveException(ex.Type, oldPage);
                if (ex.Type == ExceptionType.NoStudentCode)
                {
                    AddPage(oldPage);
                    AddException(oldPage.Student);
                }
            }
            else if (type == ResolveType.RemoveStudent)
            {
                RemovePage(oldPage, RemoveType.Student);
            }
            else
            {
                RemovePage(oldPage, RemoveType.SinglePage);
            }
        }

        public void Resolve(ExceptionList el, ResolveType type)
        {
            while (el.Exceptions.Count > 0)
                Resolve(el.Exceptions[0], type);
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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (targetException != null)
                {
                    ReplacePage(e.NewItems[0] as Page);
                    lock (Exceptions)
                    {
                        targetException = null;
                        Monitor.PulseAll(Exceptions);
                    }
                    return;
                }
                foreach (Page page in e.NewItems)
                {
                    if (page != null)
                        AddPage(page);
                }
            }
        }

        private async void LoadExercise()
        {
            try
            {
                ExerciseData = await service.GetExercise(scanModel.PageCode);
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
            ExceptionType type = CalcExcetionType(page);
            if (type != ExceptionType.None)
            {
                AddException(type, page);
            }
            if (page.Student != null)
            {
                if (page.Another != null)
                    page.Another.Student = page.Student;
                if (page.Student.AnswerPages == null)
                {
                    page.Student.AnswerPages = new List<Page>(emptyPages);
                    PageStudents.Add(page.Student);
                }
                int pageIndex = page.PageIndex / 2;
                Page old = page.Student.AnswerPages[pageIndex];
                page.Student.AnswerPages[pageIndex] = page;
                if (old != null && old != page)
                {
                    ReleasePage(old, RemoveType.DuplexPage);
                }
            }
            if (type == ExceptionType.None)
            {
                if (page.Answer != null)
                {
                    if (page.Answer.AnswerExceptions != null)
                        AddException(ExceptionType.AnswerException, page);
                    if (page.Answer.CorrectionExceptions != null)
                        AddException(ExceptionType.CorrectionException, page);
                }
                if (page.Another != null && page.Another.Answer != null)
                {
                    if (page.Another.Answer.AnswerExceptions != null)
                        AddException(ExceptionType.AnswerException, page.Another);
                    if (page.Another.Answer.CorrectionExceptions != null)
                        AddException(ExceptionType.CorrectionException, page.Another);
                }
            }
        }

        private ExceptionType CalcExcetionType(Page page)
        {
            ExceptionType type = ExceptionType.None;
            if (page.StudentCode != null)
                page.Student = schoolModel.GetStudent(page.StudentCode);
            if (page.PaperCode == null)
                type = ExceptionType.NoPageCode;
            else if (page.PaperCode != scanModel.PageCode)
                type = ExceptionType.PageCodeMissMatch;
            else if (page.Answer == null
                || (page.Another != null && page.Another.Answer == null))
                type = ExceptionType.AnalyzeException;
            else if (page.Student == null)
                type = ExceptionType.NoStudentCode;
            return type;
        }

        public void RemovePage(Page page, RemoveType type)
        {
            // 如果 Student 为 Null，肯定是 type = DuplexPage
            if (page.Student == null)
            {
                ReleasePage(page, RemoveType.DuplexPage);
                return;
            }
            if (type == RemoveType.Student)
            {
                PageStudents.Remove(page.Student);
                IList<Page> pages = page.Student.AnswerPages;
                page.Student.AnswerPages = null;
                for (int i = 0; i < pages.Count; ++i)
                {
                    if (pages[i] != null)
                    {
                        ReleasePage(pages[i], RemoveType.DuplexPage);
                    }
                }
                return;
            }
            int pageIndex = page.PageIndex / 2;
            if (page.Student.AnswerPages[pageIndex] == page)
            {
                page.Student.AnswerPages[pageIndex] = sEmptyPage;
                if (type != RemoveType.DuplexPage && page.Another != null)
                {
                    page.Student.AnswerPages[pageIndex] = page.Another;
                }
            }
            ReleasePage(page, type);
        }

        private void ReplacePage(Page page)
        {
            ExceptionType type = CalcExcetionType(page);
            if (type == ExceptionType.NoPageCode
                || type == ExceptionType.PageCodeMissMatch)
                return;
            if (targetException.Page.PageIndex != page.PageIndex 
                || (targetException.Page.StudentCode != null
                && page.StudentCode != targetException.Page.StudentCode))
                return;
            if (type == ExceptionType.AnalyzeException)
            {
                if (targetException.Page.Answer != null
                    && page.Another != null && page.Another.Answer != null)
                {
                    page.Swap(targetException.Page);
                }
                else if (page.Answer != null && targetException.Page.Another != null
                    && targetException.Page.Another.Answer != null)
                {
                    page.Another.Swap(targetException.Page.Another);
                }
                else
                {
                    return;
                }
            }
            if (targetException.Page.Student == null)
                RemovePage(targetException.Page, RemoveType.DuplexPage);
            AddPage(page);
        }

        private void ReleasePage(Page page, RemoveType type)
        {
            RemoveException(ExceptionType.None, page);
            page.Student = null;
            if (page.PagePath != null)
            {
                scanModel.ReleasePage(page);
            }
            if (page.Another != null && type == RemoveType.DuplexPage)
            {
                RemoveException(ExceptionType.None, page.Another);
                page.Another.Student = null;
                scanModel.ReleasePage(page.Another);
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
            if (type == ExceptionType.NoPageCode || type == ExceptionType.PageCodeMissMatch)
            {
                item.Index = list.Exceptions.Where(e => e.Type != ExceptionType.AnalyzeException).Count();
            }
            else if (type == ExceptionType.NoStudentCode)
            {
                item.Index = list.Exceptions.Count();
            }
        }

        private void AddException(StudentInfo s)
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

        private void RemoveException(ExceptionType type, Page page)
        {
            if (type == ExceptionType.None)
            {
                foreach (ExceptionType t in Enum.GetValues(typeof(ExceptionType)))
                    if (t != type)
                        RemoveException(t, page);
                return;
            }
            if (type == ExceptionType.AnalyzeException || type == ExceptionType.PageCodeMissMatch)
                type = ExceptionType.NoPageCode;
            ExceptionList list = Exceptions.FirstOrDefault(e => type.CompareTo(e.Type) == 0);
            if (list == null) return;
            Exception ex = list.Exceptions.FirstOrDefault(e => e.Page == page);
            if (ex == null) return;
            ex.Page = null;
            list.Exceptions.Remove(ex);
            if (list.Exceptions.Count == 0)
                Exceptions.Remove(list);
        }

    }
}
