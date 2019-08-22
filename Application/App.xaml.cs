using Account;
using Base.Boot;
using Base.Misc;
using Exercise.View;
using System;
using System.IO;
using System.Windows;
using TalBase.Model;
using TalBase.View;

namespace Application
{
    /// <summary>
    /// Application.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly Logger Log = Logger.GetLogger<App>();

        private static System.Threading.Mutex mutex;
        App()
        {
            Assistant.Fault.CrashHandler.InitUpload(Exercise.Component.DATA_PATH);
            Logger.SetLogPath(Exercise.Component.DATA_PATH);
            Logger.Config("logger.xml");
            ErrorMessageBox.Init();
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ModelBase.ShutdownAll();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "OnlyRun_CRNS");
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
                Bootstrap.Start();
            }
            else
            {
                this.Shutdown();
            }
        }
   
    }
}
