using Base.Mvvm;
using Exercise.Model;
using System.Collections.ObjectModel;
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
                SelectedExceptionList = value == null ? null : exerciseModel.Exceptions.Find(l => l.Exceptions.Contains(value));
        }
        }

        #endregion

        #region Commands

        public RelayCommand RescanCommand { get; private set; }
        public RelayCommand IgnoreCommand { get; set; }
        public RelayCommand RemovePageCommand { get; set; }
        public RelayCommand RemoveStudentCommand { get; set; }
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
            IgnoreListCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedExceptionList, ResolveType.Ignore));
            RemovePageListCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedExceptionList, ResolveType.RemovePage));
            Exceptions = exerciseModel.Exceptions;
        }

        private void Rescan(object obj)
        {
            base.Continue(obj);
        }
    }
}
