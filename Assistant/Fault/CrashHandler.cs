using Assistant.Service;
using Base.Misc;
using Base.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Fault
{
    public class CrashHandler
    {
        private static readonly Logger Log = Logger.GetLogger<CrashHandler>();

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static string logPath;
        private static CrashReport report;

        public static void Init(string path, bool main = false)
        {
            logPath = path;
            try
            {
                report = new CrashReport(path);
                if (main)
                {
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
            }
            catch
            {
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public static void AddModuleInfo(ApplicationInfo.ModuleInfo module)
        {
            report.AppInfo.Modules.Add(module);
        }

        public static async void UploadReports()
        {
            try
            {
                await Task.Run(_UploadReports);
            }
            catch (Exception e)
            {
                Log.w("UploadReports", e);
            }
        }

        private static async Task _UploadReports()
        {
            IAssistant service = Services.Get<IAssistant>();
            DirectoryInfo di = new DirectoryInfo(logPath);
            IList<string> postUrls = null;
            foreach (FileInfo f in di.GetFiles())
            {
                if (!f.Name.EndsWith(".report"))
                    continue;
                CrashReport cr = JsonPersistent.Load<CrashReport>(f.FullName);
                cr.Update2(report);
                ReportResult result = await service.FaultReport(cr);
                postUrls = result.FilePostUrls;
            }
            for (int i = 0; i < postUrls.Count; ++i)
            {
                using (FileStream fs = new FileStream(logPath + "\\" + report.Files[i], FileMode.Open, FileAccess.Read))
                {
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "application/zip");
                    HttpClient hc = new HttpClient();
                    var response = await hc.PutAsync(postUrls[i], content);
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.w(e.ExceptionObject);
            string path = logPath + "\\" + DateTime.Now.ToString("D") 
                + " " + DateTime.Now.ToString("T").Replace(':', '.') + ".report";
            report.Update(e.ExceptionObject as Exception);
            JsonPersistent.Save(path, report);
        }

    }
}
