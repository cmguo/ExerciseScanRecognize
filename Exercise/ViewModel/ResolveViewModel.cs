using Base.Mvvm;
using Exercise.Model;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    class ResolveViewModel : ViewModelBase
    {

        public RelayCommand StartScanCommand { get; private set; }

        public RelayCommand StopScanCommand { get; set; }

        public RelayCommand EndScanCommand { get; set; }

        public RelayCommand DiscardPageCommand { get; private set; }

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ResolveViewModel()
        {
            StartScanCommand = new RelayCommand((e) => scanModel.Scan(-1));
            StopScanCommand = new RelayCommand((e) => scanModel.CancelScan());
            EndScanCommand = new RelayCommand((e) => exerciseModel.MakeResult());
        }


    }
}
