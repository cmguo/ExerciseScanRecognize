using Application.Misc;

namespace Application
{
    /// <summary>
    /// Application.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        App()
        {
            ScanDeviceSaraff.init();
        }
    }
}
