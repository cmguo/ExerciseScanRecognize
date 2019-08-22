using Base.Events;
using System.Diagnostics;
using System.Management;

namespace Assistant.Online
{
    [Topic("status")]
    [External]
    public class OnlineStatus
    {
        public SystemInfo SysInfo { get; set; } = new SystemInfo();
        public ApplicationInfo AppInfo { get; set; } = new ApplicationInfo();
    }

    public partial class SystemInfo
    {
        public string OsName { get; set; }
        public string OsVersion { get; set; }
        public string SerialNo { get; set; }

        public SystemInfo()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                OsName = obj["Caption"].ToString();
                OsVersion = obj["Version"].ToString();
                SerialNo = obj["SerialNumber"].ToString();
            }
        }
    }

    public partial class ApplicationInfo
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }

        public ApplicationInfo()
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;
            Name = module.ModuleName;
            Title = module.FileVersionInfo.ProductName;
            Version = module.FileVersionInfo.FileVersion;
        }

    }
}
