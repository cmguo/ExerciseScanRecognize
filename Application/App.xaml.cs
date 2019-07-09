using Application.Misc;
using TalBase.View;

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
            Jni.Init();
        }
    }
}
