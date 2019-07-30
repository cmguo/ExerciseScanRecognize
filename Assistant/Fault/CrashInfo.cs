using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Assistant.Fault
{
    public partial class CrashInfo
    {
        public string Type { get; set; }
        public string Module { get; set; }
        public string Thread { get; set; }
        public string File { get; set; }

        public CrashInfo()
        {
        }

        public CrashInfo(string logPath, Exception crash)
        {
            Type = crash.GetType().Name;
            Module = crash.Source;
            Thread = System.Threading.Thread.CurrentThread.Name;
            if (Thread == null)
                Thread = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            string file = DateTime.Now.ToString("D")
                + " " + DateTime.Now.ToString("T").Replace(':', '.') + ".crash";
            string path = logPath + "\\" + file;
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.Write(crash);
            }
            File = file;
        }

    }

}
