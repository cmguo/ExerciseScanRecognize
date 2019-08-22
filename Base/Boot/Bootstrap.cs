using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Boot
{
    public class Bootstrap
    {

        private static BaseBootstrapper baseBootstrapper = new BaseBootstrapper();

        public static void Start()
        {
            baseBootstrapper.Run();
        }

        public static void Stop()
        {
            baseBootstrapper.Stop();
        }
    }
}
