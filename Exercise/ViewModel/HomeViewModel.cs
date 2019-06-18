using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
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
            await exerciseModel.NewTask();
            //await scanModel.Scan();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanPage());
        }
    }
}
