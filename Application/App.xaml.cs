using Account;
using Application.Misc;
using Excecise.View;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Window window = new MainWindow();
            new AccountWindow().ShowDialog();
        }

        private static readonly string ROOT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
            string path = ROOT_PATH + "\\扫描试卷\\" + DateTime.Now.ToString("D");
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
