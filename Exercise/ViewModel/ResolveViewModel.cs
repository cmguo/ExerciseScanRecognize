using Base.Mvvm;
using Exercise.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TalBase.View;
using static Exercise.Model.ExerciseModel;

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

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ResolveViewModel()
        {
            RescanCommand = new RelayCommand((e) => Rescan(e));
            IgnoreCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.Ignore));
            RemovePageCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.RemovePage));
            RemoveStudentCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.RemoveStudent));
            ResolveCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.Resolve));
            IgnoreListCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedExceptionList, ResolveType.Ignore));
            RemovePageListCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedExceptionList, ResolveType.RemovePage));
            Exceptions = exerciseModel.Exceptions;
            Exceptions.CollectionChanged += Exceptions_CollectionChanged;
            foreach (ExceptionList el in Exceptions)
            {
                el.Exceptions.CollectionChanged += Exceptions_CollectionChanged;
            }
        }

        private void Rescan(object obj)
        {
            while (!scanModel.FeederLoaded)
            {
                PopupDialog.Show("扫描仪里面没有纸张，请添加试卷。", 0, "确定");
            }
            exerciseModel.ScanOne(SelectedException);
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
                if (e.OldItems != null)
                {
                    foreach (ExceptionList el in e.OldItems)
                    {
                        el.Exceptions.CollectionChanged -= Exceptions_CollectionChanged;
                    }
                    if (e.OldItems.Contains(SelectedExceptionList))
                    {
                        int n = e.OldStartingIndex;
                        if (n >= Exceptions.Count)
                            n = 0;
                        if (Exceptions.Count > 0)
                            Selection = Exceptions[n].Exceptions[0];
                    }
                }
            }
            else
            {
                Collection<Exception> el = sender as Collection<Exception>;
                if (e.OldItems != null && e.OldItems.Contains(_SelectedException))
                {
                    int n = e.OldStartingIndex;
                    if (n >= el.Count)
                    {
                        int i = 0;
                        for (; i < Exceptions.Count; ++i)
                        {
                            if (Exceptions[i].Exceptions == el)
                            {
                                ++i;
                                break;
                            }
                        }
                        n = 0;
                        if (i >= Exceptions.Count)
                            i = 0;
                        el = Exceptions[i].Exceptions;
                    }
                    if (el.Count > 0)
                        Selection = el[n];
                }
            }
        }

    }
}
