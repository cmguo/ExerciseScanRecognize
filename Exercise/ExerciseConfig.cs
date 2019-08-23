using Base.Config;
using System.Configuration;

namespace Exercise
{

    public class ExerciseConfig : BaseSection
    {

        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty propSavePath;

        static ExerciseConfig()
        {
            propSavePath = new ConfigurationProperty("SavePath", typeof(SimpleElement<string>),
                null, ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propSavePath);
        }

        public static ExerciseConfig Instance => ConfigurationManager.GetSection("exercise") as ExerciseConfig;

        #region Properties

        protected override ConfigurationPropertyCollection Properties => properties;

        [ConfigurationProperty("SavePath")]
        public string SavePath
        {
            get => base[propSavePath] as string;
            set => base[propSavePath] = value;
        }

        #endregion

    }
}
