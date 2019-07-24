using Exercise.ViewModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

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
            KeyDown += ScanningPage_KeyDown;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsCompleted" || e.PropertyName == "IsScanning")
            {
                ScanningViewModel vm = DataContext as ScanningViewModel;
                if (vm.IsCompleted && !vm.IsScanning)
                {
                    Dispatcher.BeginInvoke((ThreadStart)delegate ()
                    {
                        vm.FinishCommand.Execute(this);
                    });
                }
            }
            else if (e.PropertyName == "Error")
            {
                Dispatcher.BeginInvoke((ThreadStart) delegate () 
                {
                    ScanningViewModel vm = DataContext as ScanningViewModel;
                    vm.ErrorCommand.Execute(this);
                });
            }
        }

        private string hake = "";

        private void ScanningPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)
                && e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
            {
                if (e.Key.CompareTo(Key.A) >= 0 && e.Key.CompareTo(Key.Z) <= 0)
                {
                    hake += (char)('A' + (int)e.Key - (int)Key.A);
                    if (hake == "QWER")
                    {
                        perf.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            else
            {
                hake = "";
            }
        }

    }
}
