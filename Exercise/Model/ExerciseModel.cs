using Base.Helpers;
using Base.Misc;
using Base.Service;
using Exercise.Algorithm;
using Exercise.Service;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalBase.Model;

namespace Exercise.Model
{
    public class ExerciseModel : ModelBase
    {
        private static readonly Logger Log = Logger.GetLogger<ExerciseModel>();

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
            NoStudentCode,
            StudentCodeMissMatch, 
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

        public string SavePath { get; private set; }
        public ObservableCollection<ExceptionList> Exceptions { get; private set; }
        public string PaperCode => scanModel.PaperCode;
        public Page LastPage { get; private set; }
        public ExerciseData ExerciseData { get; private set; }
        public System.Exception ExerciseException { get; private set; }
        public ObservableCollection<StudentInfo> PageStudents { get; private set; }

        public bool Submitting { get; private set; }

        public class ReplacePageEventArgs : CancelEventArgs
        {
            public Page Old { get; internal set; }
            public Page New { get; internal set; }
            public Page Target { get; internal set; }
        }

        public event EventHandler<ReplacePageEventArgs> BeforeReplacePage;

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
                historyModel.Clear();
        }

        public async Task NewTask()
        {
            string path = historyModel.NewRecord().LocalPath;
            scanModel.SetSavePath(path);
            SavePath = path;
            await schoolModel.Refresh();
        }

        internal void FillAll()
        {
            StudentInfo student = PageStudents.MaxItem(s => s.AnswerPages.Select(p => p.Answer != null).Count());
            schoolModel.GetNoPageStudents(s => {
                s.AnswerPages = new List<Page>(emptyPages);
                PageStudents.Add(s);
            });
            foreach (StudentInfo s in PageStudents)
            {
                for (int i = 0; i < s.AnswerPages.Count; ++i)
                {
                    if (s.AnswerPages[i] == null)
                    {
                        s.AnswerPages[i] = student.AnswerPages[i];
                    }
                    else if (s.AnswerPages[i].PagePath == null)
                    {
                        s.AnswerPages[i] = Page.EmptyPage;
                    }
                }
            }
            Exceptions.Clear();
        }

        public async Task MakeResult()
        {
            if (ExerciseData == null)
                throw new NullReferenceException("没有有效试卷信息");
            HistoryModel.Instance.BeginDuration(HistoryModel.DurationType.Summary);
            foreach (StudentInfo s in PageStudents)
            {
                AddException(s);
            }
            HistoryModel.Instance.EndDuration(PageStudents.Count);
            await Save();
        }

        public async Task SubmitResult()
        {
            string path = SavePath;
            if (!Submitting)
            {
                await Save();
                await submitModel.Save(path, PaperCode, schoolModel.Classes, PageStudents);
            }
            await submitModel.Submit(path);
            Submitting = true;
        }

        public async Task Save()
        {
            await schoolModel.Save(SavePath);
            await scanModel.Save();
            await JsonPersistent.SaveAsync(SavePath + "\\exercise.json", ExerciseData);
            await historyModel.Save();
        }

        public async Task Load(string path)
        {
            Clear();
            await schoolModel.Load(path);
            ExerciseData = await JsonPersistent.LoadAsync<ExerciseData>(path + "\\exercise.json");
            PageAnalyze.SetExerciseData(ExerciseData);
            schoolModel.GetHasPageStudents((s) =>
            {
                PageStudents.Add(s);
            });
            int n = (ExerciseData.Pages.Count + 1) / 2;
            emptyPages = new List<Page>(n);
            while (n-- > 0)
                emptyPages.Add(null);
            scanModel.SetExerciseData(ExerciseData);
            await scanModel.Load(path);
            Submitting = await submitModel.Load(path) != null;
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
            ExerciseException = null;
            PageAnalyze.SetExerciseData(null);
            LastPage = null;
            PageStudents.Clear();
            Exceptions.Clear();
            schoolModel.Clear();
            scanModel.Clear();
            SavePath = null;
            Submitting = false;
        }

        public async Task ScanOne(Exception ex)
        {
            lock (Exceptions)
            {
                targetException = ex;
            }
            try
            {
                scanModel.Scan(1);
            }
            catch
            {
                targetException = null;
                throw;
            }
            await Task.Run(() =>
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
            await scanModel.CancelScan(true);
            lock (Exceptions)
            {
                targetException = null;
                Monitor.PulseAll(Exceptions);
            }
        }

        public void Resolve(Exception ex, ResolveType type)
        {
            Page oldPage = ex.Page;
            if (type == ResolveType.Ignore)
            {
                RemoveException(ex.Type, oldPage);
                if (oldPage.Analyze != null)
                    oldPage.Analyze.ClearException(ex.Type);
            }
            else if (type == ResolveType.Resolve)
            {
                if (ex.Type == ExceptionType.NoStudentCode || ex.Type == ExceptionType.StudentCodeMissMatch)
                {
                    ReplacePage(oldPage, null);
                }
                else
                {
                    RemoveException(ex.Type, oldPage);
                }
            }
            else if (type == ResolveType.RemoveStudent)
            {
                RemovePage(oldPage, RemoveType.Student);
            }
            else
            {
                if (ex.Type == ExceptionType.PageLost)
                    RemovePage(oldPage, RemoveType.DuplexPage);
                else
                    RemovePage(oldPage, RemoveType.SinglePage);
            }
        }

        public void Resolve(ExceptionList el, ResolveType type)
        {
            while (el.Exceptions.Count > 0)
                Resolve(el.Exceptions[0], type);
        }

        public void SaveClassDetail(string fileName)
        {
            var package = new ExcelPackage();
            foreach (ClassInfo c in schoolModel.Classes.Sort())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(c.ClassName);
                ExerciseData exerciseData = ExerciseModel.Instance.ExerciseData;
                //Add the headers
                int col = 0;
                worksheet.Cells[1, ++col].Value = "姓名";
                worksheet.Cells[1, ++col].Value = "学号";
                worksheet.Cells[1, ++col].Value = "总分";
                IEnumerable<PageData.Question> questions = exerciseData.Questions;
                foreach (var q in questions)
                {
                    if (q.ItemInfo.Count == 1)
                    {
                        worksheet.Cells[1, ++col].Value = "第" + (q.Index + 1) + "题";
                    }
                    else
                    {
                        foreach (var i in q.ItemInfo)
                        {
                            worksheet.Cells[1, ++col].Value = "第" + (q.Index + 1) + "." + (i.Index + 1) + "题";
                        }
                    }
                }
                // add items
                int row = 1;
                foreach (StudentInfo s in c.Students.Where(s => s.AnswerPages != null).OrderBy(s => s.StudentNo))
                {
                    ++row;
                    col = 0;
                    worksheet.Cells[row, ++col].Value = s.Name;
                    worksheet.Cells[row, ++col].Value = s.StudentNo;
                    worksheet.Cells[row, ++col].Value = s.Score;
                    IEnumerable<double> scores = PageAnalyze.GetScoreDetail(questions, s.Answers);
                    foreach (double sc in scores)
                    {
                        if (!double.IsNaN(sc))
                            worksheet.Cells[row, ++col].Value = sc;
                        else
                            ++col;
                    }
                }
                foreach (var s in c.Students.Where(s => s.AnswerPages == null).OrderBy(s => s.StudentNo))
                {
                    ++row;
                    col = 0;
                    worksheet.Cells[row, ++col].Value = s.Name;
                    worksheet.Cells[row, ++col].Value = s.StudentNo;
                }
            }

            ProcessModule module = Process.GetCurrentProcess().MainModule;
            package.Workbook.Properties.Title = "《" + ExerciseData.Title + "》成绩表";
            package.Workbook.Properties.Author = module.FileVersionInfo.CompanyName;
            package.SaveAs(new System.IO.FileInfo(fileName));
        }

        private void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCode")
            {
                if (PaperCode == null)
                    return;
                LoadExercise();
            }
            else if (e.PropertyName == "LastPage")
            {
                Page page = scanModel.LastPage;
                if (page != null)
                {
                    if ((page.PageIndex & 1) != 0)
                        page.Analyze = PageAnalyze.Analyze(page, page.Another, true);
                    else
                        page.Analyze = PageAnalyze.Analyze(page, true);
                    if (page.Another != null)
                    {
                        page.Student = schoolModel.GetStudent(page.StudentCode);
                        LastPage = page;
                        RaisePropertyChanged("LastPage");
                    }
                }
            }
            else if (e.PropertyName == "IsCompleted")
            {
                if (scanModel.IsCompleted && targetException != null)
                {
                    lock (Exceptions)
                    {
                        targetException = null;
                        Monitor.PulseAll(Exceptions);
                    }
                }
            }
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (ExerciseData == null)
                    return;
                if (targetException != null)
                {
                    ReplaceException(e.NewItems[0] as Page);
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
                ExerciseData = await service.GetExercise(PaperCode);
                ExerciseData.QuestionTypeMap = await service.getQuestionTypeMap();
                PageAnalyze.SetExerciseData(ExerciseData);
                int n = (ExerciseData.Pages.Count + 1) / 2;
                emptyPages = new List<Page>(n);
                while (n-- > 0)
                    emptyPages.Add(null);
                RaisePropertyChanged("ExerciseData");
                scanModel.SetExerciseData(ExerciseData);
                historyModel.SetTitle(ExerciseData.Title, ExerciseData.SubjectName);
            }
            catch (System.Exception e)
            {
                ExerciseData = null;
                ExerciseException = e;
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
            if (page.PaperCode == PaperCode && page.Student != null)
            {
                if (page.Analyze == null)
                    page.Analyze = PageAnalyze.Analyze(page, false);
                if (page.Another != null)
                {
                    page.Another.Student = page.Student;
                    if (page.Another.Analyze == null)
                        page.Another.Analyze = PageAnalyze.Analyze(page.Another, page, false);
                }
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
                AnalyzePage(page);
                if (page.Another != null)
                {
                    AnalyzePage(page.Another);
                }
            }
            LastPage = page.Another == null ? page : page.Another;
            RaisePropertyChanged("LastPage");
        }

        private void AnalyzePage(Page page)
        {
            if (page.Analyze != null)
            {
                if (page.Analyze.AnswerExceptions != null)
                    AddException(ExceptionType.AnswerException, page);
                if (page.Analyze.CorrectionExceptions != null)
                    AddException(ExceptionType.CorrectionException, page);
            }
        }

        private ExceptionType CalcExcetionType(Page page)
        {
            ExceptionType type = ExceptionType.None;
            if (page.Student == null && page.StudentCode != null)
                page.Student = schoolModel.GetStudent(page.StudentCode);
            if (page.PaperCode == null)
                type = ExceptionType.NoPageCode;
            else if (page.PaperCode != PaperCode)
                type = ExceptionType.PageCodeMissMatch;
            else if (page.Answer == null
                || (page.Another != null && page.Another.Answer == null))
                type = ExceptionType.AnalyzeException;
            else if (page.StudentCode == null)
                type = ExceptionType.NoStudentCode;
            else if (page.Student == null)
                type = ExceptionType.StudentCodeMissMatch;
            return type;
        }

        public void RemovePage(Page page, RemoveType type)
        {
            // 如果 Student 为 Null，肯定是 type = DuplexPage
            if (page.PaperCode != PaperCode || page.Student == null)
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
                page.Student.AnswerPages[pageIndex] = Page.EmptyPage;
                if (type != RemoveType.DuplexPage && page.Another != null)
                {
                    page.Student.AnswerPages[pageIndex] = page.Another;
                }
                if (page.Student.AnswerPages.All(p => p == Page.EmptyPage))
                {
                    PageStudents.Remove(page.Student);
                    page.Student.AnswerPages = null;
                }
            }
            ReleasePage(page, type);
        }

        private void ReplaceException(Page page)
        {
            ExceptionType type = CalcExcetionType(page);
            if (type == ExceptionType.NoPageCode
                || type == ExceptionType.PageCodeMissMatch)
                return;
            Page tgtp = targetException.Page;
            if (tgtp.PaperCode != null
                && tgtp.PageIndex != page.PageIndex
                || tgtp.StudentCode != null
                && page.StudentCode != tgtp.StudentCode
                || tgtp.Student != null
                && page.StudentCode != tgtp.Student.TalNo)
                return;
            if (type == ExceptionType.AnalyzeException)
            {
                if (tgtp.Answer != null
                    && page.Another != null && page.Another.Answer != null)
                {
                    page.Swap(tgtp);
                }
                else if (page.Answer != null && tgtp.Another != null
                    && tgtp.Another.Answer != null)
                {
                    page.Another.Swap(tgtp.Another);
                }
                else
                {
                    return;
                }
            }
            ReplacePage(page, tgtp);
        }

        private void ReplacePage(Page page, Page tgtp)
        {
            StudentInfo student = PageStudents.Where(s => s.TalNo == page.StudentCode).FirstOrDefault();
            if (student != null)
            {
                Page old = student.AnswerPages[page.PageIndex / 2];
                if (old != null && old.PagePath != null)
                {
                    ReplacePageEventArgs args = new ReplacePageEventArgs()
                    {
                        Old = old, New = page, Target = tgtp
                    };
                    BeforeReplacePage?.Invoke(this, args);
                    if (args.Cancel)
                        return;
                }
            }
            if (tgtp == null)
                RemoveException(ExceptionType.NoStudentCode, page);
            AddPage(page);
            if (page.Student != null)
                AddException(page.Student);
            if (tgtp != null && tgtp.PagePath != null) // maybe not replaced
                ReleasePage(tgtp, RemoveType.DuplexPage);
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
                if (page.Another.PagePath != null)
                    scanModel.ReleasePage(page.Another);
            }
        }

        private void AddException(ExceptionType type, Page page)
        {
            Exception item = new Exception() { Type = type, Page = page };
            if (type == ExceptionType.AnalyzeException || type == ExceptionType.PageCodeMissMatch)
                type = ExceptionType.NoPageCode;
            if (type == ExceptionType.StudentCodeMissMatch)
                type = ExceptionType.NoStudentCode;
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
            item.Index = list.Exceptions.Where(e => e.Page.Student == null).Count();
        }

        private void AddException(StudentInfo s)
        {
            for (int i = 0; i < s.AnswerPages.Count; ++i)
            {
                if (s.AnswerPages[i] == null)
                {
                    Page p = new Page() { PaperCode = PaperCode, PageIndex = i * 2, Student = s };
                    if (p.PageIndexPlusOne < ExerciseData.Pages.Count)
                        p.Another = p;
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
            ex.Page.Exception = null;
            ex.Page = null;
            list.Exceptions.Remove(ex);
            if (list.Exceptions.Count == 0)
                Exceptions.Remove(list);
        }

    }
}
