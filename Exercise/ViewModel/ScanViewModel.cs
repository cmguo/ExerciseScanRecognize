using Exercise.Model;
using MyToolkit.Command;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        public RelayCommand StartScanCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;


        public ScanViewModel()
        {
            StartScanCommand = new RelayCommand(() => scanModel.Scan());
            StartScanCommand = new RelayCommand(() => scanModel.Scan());
        }

    }
}
