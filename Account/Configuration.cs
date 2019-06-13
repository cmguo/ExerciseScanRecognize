using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Account
{

    class Configuration : Base.Misc.Configuration
    {
        public static string StartupPage = Base.Misc.Configuration.GetByKey("StartupPage");

    }
}
