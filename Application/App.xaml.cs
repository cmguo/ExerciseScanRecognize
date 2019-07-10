using Application.Misc;
using System;
using System.Diagnostics;
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }
    }
}
