using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using Panuon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        public RelayCommand StartCommand { get; set; }

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public HomeViewModel()
        {
            StartCommand = new RelayCommand((e) => Start(e));
        }

        private async Task Start(object obj)
        {
            try
            {
                await scanModel.Open();
            }
            catch (Exception e)
            {
                PUMessageBox.ShowConfirm("扫描仪未连接，请检查后重试？", "提示", Buttons.OK);
            }
            await exerciseModel.NewTask();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanPage());
            scanModel.Scan();
        }
    }
}
