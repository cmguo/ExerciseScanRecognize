using Exercise.ViewModel;
using System.Threading;
using System.Windows.Controls;

namespace Exercise.View
{
    /// <summary>
    /// WebPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScanningPage : Page
    {
        public ScanningPage()
        {
            InitializeComponent();
            ScanningViewModel vm = DataContext as ScanningViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning")
            {
                ScanningViewModel vm = DataContext as ScanningViewModel;
                if (!vm.IsScanning)
                {
                    Dispatcher.BeginInvoke((ThreadStart)delegate ()
                    {
                        vm.FinishCommand.Execute(this);
                    });
                }
            }
            else if (e.PropertyName == "Error")
            {
                ScanningViewModel vm = DataContext as ScanningViewModel;
                Dispatcher.BeginInvoke((ThreadStart) delegate () 
                {
                    vm.ErrorCommand.Execute(this);
                });
            }
        }
    }
}
