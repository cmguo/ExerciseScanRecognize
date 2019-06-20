﻿using Base.Mvvm;
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

        public RelayCommand RescanCommand { get; private set; }
        public RelayCommand ConitnueCommand { get; private set; }
        public RelayCommand IgnoreCommand { get; set; }
        public RelayCommand RemovePageCommand { get; set; }
        public RelayCommand RemoveStudentCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ResolveViewModel()
        {
            RescanCommand = new RelayCommand((e) => scanModel.Scan(-1));
            ConitnueCommand = new RelayCommand((e) => scanModel.Scan(-1));
            IgnoreCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.Ignore));
            RemovePageCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.RemovePage));
            RemoveStudentCommand = new RelayCommand((e) => exerciseModel.Resolve(SelectedException, ResolveType.RemoveStudent));
            Exceptions = exerciseModel.Exceptions;
        }

    }
}
