using Base.Helpers;
using Exercise.Model;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        public RelayCommand StartScanCommand { get; set; }
        public RelayCommand StopScanCommand { get; set; }
        public RelayCommand EndScanCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }


        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SubmitModel submitModel = SubmitModel.Instance;


        public ScanViewModel()
        {
            StartScanCommand = new RelayCommand((e) => scanModel.Scan());
            StopScanCommand = new RelayCommand((e) => scanModel.CancelScan());
            EndScanCommand = new RelayCommand((e) => exerciseModel.MakeResult());
            SubmitCommand = new RelayCommand((e) => submitModel.Submit(null));
        }

    }
}
