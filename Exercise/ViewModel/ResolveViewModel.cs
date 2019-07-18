﻿using Base.Mvvm;
using Excecise.View;
using Exercise.Model;
using Exercise.Service;
using Exercise.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TalBase.View;
using static Exercise.Model.ExerciseModel;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.ViewModel
{
    class ResolveViewModel : ExerciseViewModel
    {

        #region Properties

        public ObservableCollection<ExceptionList> Exceptions { get; private set; }

        private ExceptionList _SelectedExceptionList;
        public ExceptionList SelectedExceptionList
        {
            get { return _SelectedExceptionList; }
            set { _SelectedExceptionList = value; RaisePropertyChanged("SelectedExceptionList"); }
        }

        private Exception _SelectedException;
        public Exception SelectedException
        {
            get { return _SelectedException; }
            set
            {
                _SelectedException = value;
                RaisePropertyChanged("SelectedException");
            }
        }

        private object _Selection;
        public object Selection
        {
            get => _Selection;
            set
            {
                if (value == _Selection)
                    return;
                _Selection = value;
                RaisePropertyChanged("Selection");
                if (value is Exception)
                {
                    SelectedException = value as Exception;
                    SelectedExceptionList = exerciseModel.Exceptions.Find(l => l.Exceptions.Contains(value as Exception));
                }
                else
                {
                    SelectedException = null;
                    SelectedExceptionList = value as ExceptionList;
                }
            }
        }

        #endregion

        #region Commands

        public RelayCommand RescanCommand { get; private set; }
        public RelayCommand IgnoreCommand { get; set; }
        public RelayCommand RemovePageCommand { get; set; }
        public RelayCommand RemoveStudentCommand { get; set; }
        public RelayCommand ResolveCommand { get; set; }
        public RelayCommand IgnoreListCommand { get; set; }
        public RelayCommand RemovePageListCommand { get; set; }

        public RelayCommand ReturnCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ResolveViewModel()
        {
            RescanCommand = new RelayCommand((e) => Rescan(e));
            IgnoreCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.Ignore));
            RemovePageCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.RemovePage));
            RemoveStudentCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.RemoveStudent));
            ResolveCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.Resolve));
            IgnoreListCommand = new RelayCommand((e) => Resolve(e, SelectedExceptionList, ResolveType.Ignore));
            RemovePageListCommand = new RelayCommand((e) => Resolve(e, SelectedExceptionList, ResolveType.RemovePage));
            Exceptions = exerciseModel.Exceptions;
            Exceptions.CollectionChanged += Exceptions_CollectionChanged;
            ReturnCommand = new RelayCommand((o) => Return(o));
            foreach (ExceptionList el in Exceptions)
            {
                el.Exceptions.CollectionChanged += Exceptions_CollectionChanged;
            }
        }

        public override void Release()
        {
            base.Release();
            Exceptions.CollectionChanged -= Exceptions_CollectionChanged;
            foreach (ExceptionList el in Exceptions)
            {
                el.Exceptions.CollectionChanged -= Exceptions_CollectionChanged;
            }
        }

        public void InitSelection()
        {
            if (Exceptions.Count > 0)
                Selection = Exceptions[0].Exceptions[0];
        }

        private async Task Rescan(object obj)
        {
            int result = 0;
            while (result == 0 && !scanModel.FeederLoaded)
            {
                result = PopupDialog.Show(obj as UIElement, "TODO", "扫描仪里面没有纸张，请添加试卷。", 0, "确定", "取消");
            }
            if (result != 0)
                return;
            Exception ex = SelectedException;
            await exerciseModel.ScanOne(ex);
            if (ex.Page != null)
            {
                TalToast.Show("X该试卷仍无法识别，请检查后重新扫描");
            }
            else
            {
                TalToast.Show("该份试卷的异常已处理完成");
            }
            if (Exceptions.Count == 0)
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

        private async Task Resolve(object obj, ExerciseModel.Exception exception, ResolveType type)
        {
            string title = null;
            string message = null;
            string btn = null;
            switch (type)
            {
                case ResolveType.Ignore:
                case ResolveType.RemovePage:
                    title = "忽略异常";
                    message = "忽略后，该张试卷所有题目作答将无法统计，确认忽略吗？";
                    btn = "忽略";
                    break;
            }
            if (title != null)
            {
                int n = PopupDialog.Show(obj as FrameworkElement, title, message, 0, btn, "取消");
                if (n == 1)
                    return;
            }
            exerciseModel.Resolve(exception, type);
            if (Exceptions.Count == 0)
            {
                await exerciseModel.Save();
                TalToast.Show("异常已全部处理完成");
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
            else
            {
                string toast = null;
                switch (type)
                {
                    case ResolveType.Ignore:
                    case ResolveType.RemovePage:
                        toast = "该份试卷的异常已忽略";
                        break;
                    case ResolveType.Resolve:
                        toast = "该份试卷的异常已处理完成";
                        break;
                }
                TalToast.Show(toast);
            }
        }

        private void Resolve(object obj, ExerciseModel.ExceptionList list, ResolveType type)
        {
            exerciseModel.Resolve(list, type);
            if (Exceptions.Count == 0)
            {
                TalToast.Show("异常已全部处理完成");
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
        }

        private async Task Return(object obj)
        {
            await exerciseModel.CancelScanOne();
            await exerciseModel.Save();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

        private void Exceptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender == Exceptions)
            {
                if (e.NewItems != null)
                {
                    foreach (ExceptionList el in e.NewItems)
                    {
                        el.Exceptions.CollectionChanged += Exceptions_CollectionChanged;
                    }
                }
                //if (e.OldItems != null)
                //{
                //    foreach (ExceptionList el in e.OldItems)
                //    {
                //        el.Exceptions.CollectionChanged -= Exceptions_CollectionChanged;
                //    }
                //    if (e.OldItems.Contains(SelectedExceptionList))
                //    {
                //        int n = e.OldStartingIndex;
                //        if (n >= Exceptions.Count)
                //            n = 0;
                //        if (Exceptions.Count > 0)
                //            Selection = Exceptions[n].Exceptions[0];
                //    }
                //}
            }
            else
            {
                //Collection<Exception> el = sender as Collection<Exception>;
                //if (e.OldItems != null && e.OldItems.Contains(_SelectedException))
                //{
                //    int n = e.OldStartingIndex;
                //    if (n >= el.Count)
                //    {
                //        int i = 0;
                //        for (; i < Exceptions.Count; ++i)
                //        {
                //            if (Exceptions[i].Exceptions == el)
                //            {
                //                ++i;
                //                break;
                //            }
                //        }
                //        n = 0;
                //        if (i >= Exceptions.Count)
                //            i = 0;
                //        el = Exceptions[i].Exceptions;
                //    }
                //    if (el.Count > 0)
                //        Selection = el[n];
                //}
            }
        }

    }
}
