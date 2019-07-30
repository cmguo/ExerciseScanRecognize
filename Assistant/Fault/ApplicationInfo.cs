using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Fault
{
    public partial class ApplicationInfo
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public IList<ModuleInfo> Modules { get; set; }

        public partial class ModuleInfo
        {
            public string Name { get; set; }
            public string Version { get; set; }
        }

        public ApplicationInfo()
        {
        }

        public ApplicationInfo(int unused)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;
            Name = module.ModuleName;
            Title = module.FileVersionInfo.ProductName;
            Version = module.FileVersionInfo.FileVersion;
            Modules = new List<ModuleInfo>();
        }

        public void Update()
        {
            Modules = Modules.Concat(Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Select(m => new ModuleInfo() { Name = m.FileName, Version = m.FileVersionInfo.FileVersion }))
                .ToList();
        }
    }
}
