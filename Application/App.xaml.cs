using Account;
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
            Logger.SetLogPath(Exercise.Component.DATA_PATH);
            Logger.Config("logger.xml");
            ErrorMessageBox.Init();
            //Misc.Jni.Init();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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
                Window window = new MainWindow();
                new AccountWindow().ShowDialog();
            }

            else
            {
                this.Shutdown();
            }
         
        }
   

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.w(e.ExceptionObject);
            string path = Exercise.Component.DATA_PATH + DateTime.Now.ToString("D");
            Directory.CreateDirectory(path);
            path += "\\" + DateTime.Now.ToString("T").Replace(':', '.') + ".crash";
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(e.ExceptionObject);
            }
        }
    }
}
