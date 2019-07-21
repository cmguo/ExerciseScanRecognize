using System.Configuration;

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
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
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
