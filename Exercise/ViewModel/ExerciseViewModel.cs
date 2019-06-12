using Base.Helpers;
using Exercise.Model;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    class ExerciseViewModel : ViewModelBase
    {

        public RelayCommand StartScanCommand { get; private set; }

        public RelayCommand StopScanCommand { get; set; }

        public RelayCommand EndScanCommand { get; set; }

        public RelayCommand DiscardPageCommand { get; private set; }

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public ExerciseViewModel()
        {
            StartScanCommand = new RelayCommand((e) => scanModel.Scan(-1));
            StopScanCommand = new RelayCommand((e) => scanModel.CancelScan());
            EndScanCommand = new RelayCommand((e) => exerciseModel.MakeResult());
        }


    }
}
