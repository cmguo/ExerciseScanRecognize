using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Threading.Tasks;
using TalBase.Utils;
using System.Windows.Navigation;

namespace Exercise.ViewModel
{
    public class HomeViewModel : ScanViewModel
    {
        public string[] SourceList { get; internal set; }

        #region Commands

        public RelayCommand StartCommand { get; set; }
        public RelayCommand HistroyCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public HomeViewModel()
        {
            StartCommand = new RelayCommand((e) => Start(e));
            HistroyCommand = new RelayCommand((e) => History(e));
            SourceList = scanModel.SourceList;
        }

        private async Task Start(object obj)
        {
            NetWorkManager.CheckNetWorkAvailable("当前电脑无网络连接，请检查后再开始扫描。");
            await exerciseModel.NewTask();
            if (base.Continue(obj))
            {
                NavigationService navigationService = (obj as System.Windows.Controls.Page).NavigationService;
                navigationService.Navigate(new ScanningPage());
            }
            else
            {
                exerciseModel.Discard();
            }
        }

        private void History(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HistoryPage());
        }

    }
}
