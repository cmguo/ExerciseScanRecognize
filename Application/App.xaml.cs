using Application.Misc;
using Exercise.View;

namespace Application
{
    /// <summary>
    /// Application.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        App()
        {
            ErrorMessageBox.Init();
            ScanDeviceSaraff.Init();
            Jni.Init();
        }
    }
}
