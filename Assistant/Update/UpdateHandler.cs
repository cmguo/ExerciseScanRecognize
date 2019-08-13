using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using Base.Events;
using TalBase.View;

namespace Assistant.Update
{
    public class UpdateHandler
    {

        private static string logPath;
        private static Event<UpdateMessage> @event = EventBus.Instance.GetEvent<Event<UpdateMessage>>();
        private static Event<string> @event2 = EventBus.Instance.GetEvent<Event<string>>();

        private static UpdateMessage message;

        public static void Init(string path)
        {
            logPath = path;
            @event.Subscribe(OnUpdate, Prism.Events.ThreadOption.UIThread);
        }

        private static async void OnUpdate(UpdateMessage msg)
        {
            string setup = logPath + "\\" + "Setup." + msg.Version + ".msi";
            HttpClient hc = new HttpClient();
            using (Stream his = await hc.GetStreamAsync(msg.Url))
            using (Stream fos = File.Open(setup, FileMode.OpenOrCreate))
            {
                his.CopyTo(fos);
            }
            message.Url = setup;
            if (AppStatus.Instance.Busy)
            {
                AppStatus.Instance.PropertyChanged += AppStatus_PropertyChanged;
                message = msg;
                return;
            }
            Update();
        }

        private static void AppStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Busy" && !AppStatus.Instance.Busy)
            {
                AppStatus.Instance.PropertyChanged -= AppStatus_PropertyChanged;
                Update();
            }
        }

        private static void Update()
        {
            int result = PopupDialog.Show(Application.Current.MainWindow,
                "更新软件", "已有新的版本，是否更新？", 0, "更新", "不更新");
            if (result != 0)
            {
                return;
            }
            string setup = message.Url;
            string bat = setup + ".bat";
            string script = "msiexec /x " + setup + "/q\r\n";
            script += "msiexec /i " + setup + "\r\n";
            using (Stream fos = File.Open(setup, FileMode.OpenOrCreate))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(script);
                fos.Write(bytes, 0, bytes.Length);
                fos.Flush();
            }
            Process proc = new Process();
            proc.StartInfo.FileName = bat;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            Application.Current.MainWindow.Close();
        }
    }
}
