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
            StartScanCommand = new RelayCommand(() => Scan());
            StartScanCommand = new RelayCommand(() => Scan());
        }

        public void Scan(short count = -1)
        {
            scanModel.Scan(count);
        }

        private void ScanDevice_GetFileName(object sender, ImageEvent e)
        {
            
        }

        private void ScanDevice_OnImage(object sender, ImageEvent e)
        {
        }

    }
}
