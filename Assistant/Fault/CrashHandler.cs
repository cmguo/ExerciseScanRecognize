using Assistant.Service;
using Base.Misc;
using Base.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assistant.Fault
{
    [Export("crash")]
    public class CrashHandler : Base.Boot.IAssistant
    {
        private static readonly Logger Log = Logger.GetLogger<CrashHandler>();

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static CrashReport report;

        public static void InitUpload(string logPath)
        {
            try
            {
                report = new CrashReport(logPath);
                int n = 0;
                DirectoryInfo di2 = new DirectoryInfo(logPath);
                foreach (FileInfo f in di2.GetFiles())
                {
                    if (f.Name.EndsWith(".zip"))
                        f.Delete();
                    if (f.Name.EndsWith(".report"))
                        ++n;
                }
                if (n > 0)
                    report.Update2();
            }
            catch
            {
            }
        }

        private string logPath;

        [ImportingConstructor]
        public CrashHandler(Base.Boot.IProduct product)
        {
            logPath = product.LogPath;
            if (report == null)
                report = new CrashReport();
            else
                AppStatus.Instance.PropertyChanged += AppStatus_PropertyChanged;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void AddModuleInfo(ApplicationInfo.ModuleInfo module)
        {
            report.AppInfo.Modules.Add(module);
        }

        private async void AppStatus_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Busy" || AppStatus.Instance.Busy)
            {
                return;
            }
            try
            {
                await Task.Run(UploadReports);
                AppStatus.Instance.PropertyChanged -= AppStatus_PropertyChanged;
            }
            catch (Exception ex)
            {
                Log.w("UploadReports", ex);
            }
        }

        private async Task UploadReports()
        {
            IAssistant service = Services.Get<IAssistant>();
            DirectoryInfo di = new DirectoryInfo(logPath);
            IDictionary<string, string> postUrls = null;
            foreach (FileInfo f in di.GetFiles())
            {
                if (!f.Name.EndsWith(".report"))
                    continue;
                CrashReport cr = JsonPersistent.Load<CrashReport>(f.FullName);
                cr.Update2(report);
                ReportResult result = await service.FaultReport(cr);
                postUrls = result.FilePostUrls;
                if (postUrls == null)
                    throw new NullReferenceException("postUrls == null");
            }
            for (int i = 0; i < report.Files.Count; ++i)
            {
                using (FileStream fs = new FileStream(logPath + "\\" + report.Files[i], FileMode.Open, FileAccess.Read))
                {
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "application/zip");
                    HttpClient hc = new HttpClient();
                    var response = await hc.PutAsync(postUrls[report.Files[i]], content);
                    if (response.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                        throw new HttpResponseException(response.StatusCode, response.ReasonPhrase);
                }
            }
            DirectoryInfo di2 = new DirectoryInfo(logPath);
            foreach (FileInfo f in di2.GetFiles())
            {
                if (f.Name.EndsWith(".report")
                    || f.Name.EndsWith(".crash")
                    || f.Name.EndsWith(".zip"))
                    f.Delete();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.w(e.ExceptionObject);
            string path = logPath + "\\" + DateTime.Now.ToString("D") 
                + " " + DateTime.Now.ToString("T").Replace(':', '.') + ".report";
            report.Update(e.ExceptionObject as Exception);
            JsonPersistent.Save(path, report);
        }

    }
}
