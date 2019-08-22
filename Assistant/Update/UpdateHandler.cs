using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using Base.Boot;
using Base.Events;
using TalBase.View;

namespace Assistant.Update
{

    [Export("update")]
    [InheritedExport(typeof(IAssistant)),
        ExportMetadata("MainProcessOnly", true)]
    public class UpdateHandler : IAssistant
    {

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
            //@event.Publish(new UpdateMessage());
        }

        private async void OnUpdate(UpdateMessage msg)
        {
            string setup = savePath + "\\" + "Setup." + msg.Version + ".msi";
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
            int result = PopupDialog.Show(Application.Current.MainWindow,
                "更新软件", "已有新的版本，是否更新？", 0, "更新", "不更新");
            if (result != 0)
            {
                return;
            }
            string setup = message.Url;
            string bat = setup + ".bat";
            string script = "msiexec /x '" + productCode +  "' /q\r\n";
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
