using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using Panuon.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Navigation;
using TalBase.ViewModel;
using TalBase.Utils;

namespace Exercise.ViewModel
{
    public class HomeViewModel : ScanViewModel
    {
        public string[] SourceList { get; }

        public int SourceIndex { get => scanModel.SourceIndex; set => scanModel.SourceIndex = value; }

        public RelayCommand StartCommand { get; set; }

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public HomeViewModel()
        {
            StartCommand = new RelayCommand((e) => Start(e));
            try
            {
                SourceList = scanModel.SourceList;
            }
            catch
            {
            }
        }

        private async Task Start(object obj)
        {
            if (!NetWorkManager.CheckNetWorkAvailable())
            {
                return;
            }
            try
            {
                SourceIndex = SourceIndex;
            }
            catch
            {
                PUMessageBox.ShowConfirm("扫描仪未连接，请检查后重试？", "提示", Buttons.OK);
                return;
            }
            await exerciseModel.NewTask();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanningPage());
            base.Continue(obj);
        }
    }
}
