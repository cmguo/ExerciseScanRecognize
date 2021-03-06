using Prism.Logging;
using Prism.Mef;
using Prism.Modularity;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Windows;

namespace Base.Boot
{
    public class BaseBootstrapper : MefBootstrapper
    {

        protected override ILoggerFacade CreateLogger()
        {
            return new LoggerFacade();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return base.CreateModuleCatalog();
        }

        protected override AggregateCatalog CreateAggregateCatalog()
        {
            return base.CreateAggregateCatalog();
        }

        public new void Run()
        {
            base.Run();
        }

        public void Stop()
        {
            if (Container != null)
                Container.Dispose();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            string modules = ConfigurationManager.AppSettings.Get("AggregateModules");
            if (modules == null)
            {
                DirectoryCatalog catalog = new DirectoryCatalog(".");
                DirectoryCatalog catalog2 = new DirectoryCatalog(".", "*.exe");
                AggregateCatalog.Catalogs.Add(catalog);
                AggregateCatalog.Catalogs.Add(catalog2);
            }
            else
            {
                foreach (string m in modules.Split(','))
                {
                    AggregateCatalog.Catalogs.Add(new AssemblyCatalog(m));
                }
            }
        }

        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void InitializeShell()
        {
            (Shell as Shell).Initialize();
        }

    }
}
