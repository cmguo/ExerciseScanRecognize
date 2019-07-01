using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Base.Misc
{

    public class Configuration
    {
        public static string GetByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static void SetByKey(string key, string value)
        {
            config.AppSettings.Settings[key].Value = value;
            Save();
        }

        private static System.Configuration.Configuration config =
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private static void Save()
        {
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
