using Base.Mvvm;
using Exercise.Model;
using System.Collections.ObjectModel;
using TalBase.ViewModel;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    class ResolveViewModel : ViewModelBase
    {

        #region Properties

        public ObservableCollection<ExceptionList> Exceptions { get; private set; }
        private Exception _SelectedException;
        public Exception SelectedException
        {
            get { return _SelectedException; }
            set { _SelectedException = value; RaisePropertyChanged("SelectedException"); }
        }

        #endregion

        #region Commands

        public RelayCommand ConitnueCommand { get; private set; }
        public RelayCommand IgnoreCommand { get; set; }
        public RelayCommand DropCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ResolveViewModel()
        {
            ConitnueCommand = new RelayCommand((e) => scanModel.Scan(-1));
            IgnoreCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.Ignore));
            DropCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.RemovePage));
            Exceptions = exerciseModel.Exceptions;
        }

    }
}
