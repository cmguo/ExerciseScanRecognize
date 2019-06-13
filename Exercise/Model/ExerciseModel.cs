﻿using Base.Service;
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

        public class Exception
        {
            public Object Object { get; set; }
            public string Message { get; set; }
        }

        public class ExceptionList
        {
            public ExceptionType Type { get; set; }
            public ObservableCollection<Exception> Exceptions;
        }

        private SchoolModel schoolModel = SchoolModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;
        private IExercise service;

        private SubmitModel.SubmitTask task;
        public ObservableCollection<ExceptionList> Exceptions = new ObservableCollection<ExceptionList>();
        public ObservableCollection<Page> PageDropped = new ObservableCollection<Page>();
        private List<Page> emptyPages;
        private ExerciseData exerciseData;

        public ExerciseModel()
        {
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            scanModel.PropertyChanged += ScanModel_PropertyChanged;
            service = Services.Get<IExercise>();
        }

        public void NewTask()
        {
            task = SubmitModel.Instance.AddTask();
            scanModel.SetSavePath(task.SavePath);
        }

        public void MakeResult()
        {
            schoolModel.GetLostPageStudents(s => AddException(ExceptionType.PageLost, s));
        }

        public void SubmitTask()
        {
            SubmitModel.Instance.Submit(task);
            task = null;
            emptyPages = null;
            exerciseData = null;
            Exceptions.Clear();
            PageDropped.Clear();
            scanModel.ClearPages();
            schoolModel.Clear();
        }

        private async void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCode")
            {
                exerciseData = await service.GetExercise(scanModel.PageCode);
                emptyPages = new List<Page>();
                while (emptyPages.Count < (exerciseData.pages.Count() + 1) / 2)
                    emptyPages.Add(null);
                scanModel.SetExerciseData(exerciseData);
            }
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Page page = e.NewItems[0] as Page;
            AddPage(page);
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
            }
            else
            {
                if (page.Student.AnswerPages == null)
                {
                    page.Student.AnswerPages = new List<Page>(emptyPages);
                }
                int pageIndex = page.PageIndex / 2;
                if (page.Student.AnswerPages[pageIndex] != null)
                {
                    Page drop = page.Student.AnswerPages[pageIndex];
                    RemovePage(drop);
                }
                page.Student.AnswerPages[pageIndex] = page;
                if (page.Student.AnswerPages.IndexOf(null) < 0)
                {
                    RemoveException(ExceptionType.PageLost, page.Student);
                }
            }
            if (page.Answer == null)
            {
                RemoveException(ExceptionType.AnalyzeException, page);
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

        private void RemovePage(Page page)
        {
            if (page.Student != null)
            {
                int pageIndex = page.PageIndex / 2;
                if (page.Student.AnswerPages[pageIndex] == page)
                    page.Student.AnswerPages[pageIndex] = null;
            }
            RemoveException(ExceptionType.None, page);
            PageDropped.Add(page);
        }

        private void UpdatePage(ExceptionType type, Page page)
        {
            RemoveException(type, page);
            if (type == ExceptionType.NoStudentCode)
            {
                AddPage(page);
            }
        }

        private void AddException(ExceptionType type, Object obj)
        {
            ExceptionList list = Exceptions.FirstOrDefault(e => type.CompareTo(e.Type) <= 0);
            if (list != null && list.Type == type)
            {
                list.Exceptions.Add(new Exception() { Object = obj });
            }
        }

        private void RemoveException(ExceptionType type, Object obj)
        {
            if (type == ExceptionType.None)
            {
                foreach (ExceptionType t in Enum.GetValues(typeof(ExceptionType)))
                    if (t != type)
                        RemoveException(t, obj);
                return;
            }
            ExceptionList list = Exceptions.FirstOrDefault(e => type.CompareTo(e.Type) == 0);
            if (list == null) return;
            Exception ex = list.Exceptions.FirstOrDefault(e => e.Object == obj);
            if (ex == null) return;
            list.Exceptions.Remove(ex);
            if (list.Exceptions.Count == 0)
                Exceptions.Remove(list);
        }

    }
}
