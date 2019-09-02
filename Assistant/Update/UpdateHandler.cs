using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Base.Boot;
using Base.Events;
using Base.Misc;
using TalBase;
using TalBase.View;

namespace Assistant.Update
{

    [Export("update")]
    [InheritedExport(typeof(IAssistant)),
        ExportMetadata("MainProcessOnly", true)]
    public class UpdateHandler : IAssistant
    {

        private static readonly Logger Log = Logger.GetLogger<UpdateHandler>();

        private string savePath;
        private string productCode;
        private EventBus eventBus;

        private Event<UpdateMessage> @event;

        private UpdateMessage message;

        [ImportingConstructor]
        public UpdateHandler(IProduct product, EventBus bus)
        {
            savePath = product.LogPath;
            productCode = product.ProductCode;
            eventBus = bus;
            @event = eventBus.GetEvent<Event<UpdateMessage>>();
            @event.Subscribe(OnUpdate, Prism.Events.ThreadOption.UIThread);
            if (File.Exists(savePath + "\\update.json"))
            {
                UpdateMessage msg = JsonPersistent.Load<UpdateMessage>(savePath + "\\update.json");
                OnUpdate(msg);
                return;
            }
            /*
            @event.Publish(new UpdateMessage()
            {
                Version = "1.1.0",
                Url = "http://10.145.109.3/ExerciseSetup.msi",
                Md5 = "9bd9d0d0c5a683c5d15713db33585c8d"
            });
            */
        }

        private async void OnUpdate(UpdateMessage msg)
        {
            await JsonPersistent.SaveAsync(savePath + "\\update.json", msg);
            string setup = savePath + "\\" + "Setup." + msg.Version + ".msi";
            string setup2 = setup + ".temp";
            if (!File.Exists(setup))
            {
                Log.d("Start download " + msg.Url);
                HttpClient hc = new HttpClient();
                long size = 0;
                if (File.Exists(setup2))
                    size = new FileInfo(setup2).Length;
                hc.DefaultRequestHeaders.Range = new RangeHeaderValue(size, null);
                using (Stream his = await hc.GetStreamAsync(msg.Url))
                using (Stream fos = File.Open(setup2, FileMode.OpenOrCreate | FileMode.Append))
                {
                    await his.CopyToAsync(fos);
                }
                File.Move(setup2, setup);
            }
            using (FileStream fs = new FileStream(setup, FileMode.Open, FileAccess.Read))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(fs);
                string digest = BitConverter.ToString(output).Replace("-", "").ToLower();
                if (digest != msg.Md5)
                {
                    Log.w("File md5 not match " + digest + " => " + msg.Md5);
                    msg = null;
                }
            }
            if (msg == null)
            {
                File.Delete(setup);
                return;
            }
            msg.Url = setup;
            msg.Md5 = null;
            message = msg;
            if (!AppStatus.Instance.Free)
            {
                Log.d("Wait idle " + msg.Url);
                AppStatus.Instance.PropertyChanged += AppStatus_PropertyChanged;
                return;
            }
            Update();
        }

        private void AppStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Free" && AppStatus.Instance.Free)
            {
                AppStatus.Instance.PropertyChanged -= AppStatus_PropertyChanged;
                Update();
            }
        }

        private void Update()
        {
            Log.d("Start update " + message.Url);
            int result = PopupDialog.Show(Application.Current.MainWindow,
                "更新软件", "已有新的版本，是否更新？", 0, "更新", "不更新");
            if (result != 0)
            {
                return;
            }
            string setup = message.Url;
            string bat = setup + ".bat";
            string script = "msiexec /x " + productCode +  " /q\r\n";
            script += "msiexec /i " + setup + "\r\n";
            script += "del " + setup + "\r\n";
            script += "del " + savePath + "\\update.json\r\n";
            script += "start \"\" \"" + Process.GetCurrentProcess().MainModule.FileName + "\"\r\n";
            script += "(goto) 2>nul & del \"%~f0\"\r\n"; // delete self
            using (Stream fos = File.Open(bat, FileMode.OpenOrCreate))
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
