using Base.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Assistant.Fault
{
    public partial class CrashReport
    {
        public long Time { get; set; }
        public SystemInfo SysInfo { get; set; }
        public ApplicationInfo AppInfo { get; set; }
        public UserInfo UserInfo { get; set; }
        public CrashInfo CrashInfo { get; set; }
        public IList<string> Files { get; set; }

        public CrashReport()
        {
        }

        public CrashReport(string path)
        {
            SysInfo = new SystemInfo(path);
            AppInfo = new ApplicationInfo(0);
            UserInfo = new UserInfo()
            {
                Id = Account.Model.AccountModel.Instance.Account.Id.ToString(),
                Name = Account.Model.AccountModel.Instance.Account.Name,
            };
        }

        public void Update(Exception crash)
        {
            Time = DateTime.Now.Timestamp();
            SysInfo.Update();
            AppInfo.Update();
            CrashInfo = new CrashInfo(SysInfo.Storage.Path, crash);
        }

        public void Update2()
        {
            SysInfo.Update2();
            string temp = Path.GetTempFileName();
            using (FileStream zipToOpen = new FileStream(temp, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                DirectoryInfo di = new DirectoryInfo(SysInfo.Storage.Path);
                foreach (FileInfo f in di.GetFiles())
                {
                    if (!f.Name.Contains(".log") && !f.Name.EndsWith(".crash"))
                        continue;
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(f.Name);
                    using (Stream os = readmeEntry.Open())
                    using (FileStream log = new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        log.CopyTo(os);
                    }
                }
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream zipToOpen = new FileStream(temp, FileMode.Open))
                {
                    md5.ComputeHash(zipToOpen);
                }
                string hash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
                File.Move(temp, SysInfo.Storage.Path + "\\" + hash + ".zip");
                Files = new string[] { hash + ".zip" };
            }
        }

        public void Update2(CrashReport report)
        {
            SysInfo.Update2(report.SysInfo);
            Files = report.Files;
        }
    }
}
