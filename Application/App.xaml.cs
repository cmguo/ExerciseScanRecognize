using Application.Misc;
using Exercise;

namespace Application
{
    /// <summary>
    /// Application.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        App()
        {
            ErrorWindow.Init();
            ScanDeviceSaraff.Init();
            Jni.Init();
        }
    }
}
