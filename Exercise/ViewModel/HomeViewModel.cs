using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using Panuon.UI;
using System;
using System.Threading.Tasks;
using TalBase.Utils;
using System.Diagnostics;
using TalBase.View;
using System.Windows;
using System.Windows.Navigation;

namespace Exercise.ViewModel
{
    public class HomeViewModel : ScanViewModel
    {
        public string[] SourceList { get; }

        public int SourceIndex { get => scanModel.SourceIndex; set => scanModel.SourceIndex = value; }

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
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                PopupDialog.Show(obj as UIElement, "TODO", "扫描仪未连接，请检查后重试。", 0, "确定");
                return;
            }
            await exerciseModel.NewTask();
            NavigationService navigationService = (obj as System.Windows.Controls.Page).NavigationService;
            navigationService.Navigate(new ScanningPage());
            if (!base.Continue(obj))
                navigationService.Navigate(new HomePage());
        }

        private void History(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HistoryPage());
        }

    }
}
