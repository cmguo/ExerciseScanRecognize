using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace Assistant.Fault
{
    public partial class SystemInfo
    {
        public string OsName { get; set; }
        public string OsVersion { get; set; }
        public string SerialNo { get; set; }
        public CpuInfo Cpu { get; set; }
        public MemoryInfo Memory { get; set; }
        public StorageInfo Storage { get; set; }

        public partial class CpuInfo
        {
            public string Name { get; set; }
            public int Kernel { get; set; }
            public int Freqence { get; set; }

            public CpuInfo()
            {
            }

            public CpuInfo(int unused)
            {
                Task.Run(() =>
                {
                    using (var managementObject = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
                    {
                        Name = managementObject["Name"].ToString();
                        Kernel = Int32.Parse(managementObject["NumberOfCores"].ToString());
                        Freqence = Int32.Parse(managementObject["MaxClockSpeed"].ToString()) * 1024 * 1024;
                    }
                });
            }
        }

        public partial class MemoryInfo
        {
            public long Total { get; set; }
            public long Free { get; set; }
            public long Self { get; set; }

            public MemoryInfo()
            {
            }

            public MemoryInfo(int unused)
            {
                Task.Run(() =>
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_PhysicalMemory");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        Total = Int64.Parse(obj["Capacity"].ToString());
                    }
                });
            }

            public void Update()
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    Free = Int64.Parse(obj["FreePhysicalMemory"].ToString()) * 1024;
                }
                Self = Environment.WorkingSet;
            }
        }

        public partial class StorageInfo
        {
            public string Path { get; set; }
            public long Total { get; set; }
            public long Free { get; set; }
            public long Self { get; set; }

            public StorageInfo()
            {
            }

            public StorageInfo(string path)
            {
                Path = path;
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    if (path.StartsWith(d.Name))
                    {
                        Total = d.TotalSize;
                        Free = d.TotalFreeSpace;
                        break;
                    }
                }
            }

            public void Update()
            {
                Self = DirSize(new DirectoryInfo(Path));
            }

            public void Update2(StorageInfo storage)
            {
                Self = storage.Self;
            }

            private static long DirSize(DirectoryInfo d)
            {
                long size = 0;
                // Add file sizes.
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }
                return size;
            }

        }

        public SystemInfo()
        {
        }

        public SystemInfo(string path)
        {
            Task.Run(() =>
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    OsName = obj["Caption"].ToString();
                    OsVersion = obj["Version"].ToString();
                    SerialNo = obj["SerialNumber"].ToString();
                }
            });
            Cpu = new CpuInfo(0);
            Memory = new MemoryInfo(0);
            Storage = new StorageInfo(path);
        }

        public void Update()
        {
            Memory.Update();
        }

        public void Update2()
        {
            Storage.Update();
        }

        public void Update2(SystemInfo sysInfo)
        {
            Storage.Update2(sysInfo.Storage);
        }

    }
}
