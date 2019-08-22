using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Boot
{
    public class Bootstrap
    {

        public static void Start()
        {
            new BaseBootstrapper().Run();
        }
    }
}
