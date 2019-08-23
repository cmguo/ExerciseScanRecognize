using Base.Config;
using System.Configuration;

namespace Account
{

    public class ServiceUriElement : Element
    {
        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty propName;
        private static ConfigurationProperty propValue;

        static ServiceUriElement()
        {
            propName = new ConfigurationProperty("Name", typeof(string),
                null, ConfigurationPropertyOptions.IsRequired);
            propValue = new ConfigurationProperty("Value", typeof(string),
                null, ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propName);
            properties.Add(propValue);
        }

        protected override ConfigurationPropertyCollection Properties => properties;

        public override object Key => base[propName];

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name => base[propName] as string;

        [ConfigurationProperty("Value", IsRequired = true)]
        public string Value => base[propValue] as string;
    }

    [ConfigurationCollection(typeof(ServiceUriElement),
        CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class ServiceUriCollection : ElementCollection<ServiceUriElement>
    {

    }

    public class AccountConfig : BaseSection
    {

        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty propStartupWindow;
        private static ConfigurationProperty propServiceUri;
        private static ConfigurationProperty propServiceUris;
        private static ConfigurationProperty propAccountName;

        static AccountConfig()
        {
            propStartupWindow = new ConfigurationProperty("StartupWindow", typeof(SimpleElement<string>),
                null, ConfigurationPropertyOptions.IsRequired);
            propServiceUri = new ConfigurationProperty("ServiceUri", typeof(SimpleElement<string>),
                null, ConfigurationPropertyOptions.IsRequired);
            propServiceUris = new ConfigurationProperty("ServiceUris", typeof(ServiceUriCollection),
                null, ConfigurationPropertyOptions.None);
            propAccountName = new ConfigurationProperty("AccountName", typeof(SimpleElement<string>),
                null, ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propStartupWindow);
            properties.Add(propServiceUri);
            properties.Add(propServiceUris);
            properties.Add(propAccountName);
        }

        public static AccountConfig Instance => ConfigurationManager.GetSection("account") as AccountConfig;

        #region Properties

        protected override ConfigurationPropertyCollection Properties => properties;

        [ConfigurationProperty("StartupWindow", IsRequired = true)]
        public string StartupWindow => base[propStartupWindow] as string;

        [ConfigurationProperty("ServiceUri", IsRequired = true)]
        public string ServiceUri
        {
            get => base[propServiceUri] as string;
            set => base[propServiceUri] = value;
        }

        [ConfigurationProperty("ServiceUris", IsRequired = false)]
        public ServiceUriCollection ServiceUris => base[propServiceUris] as ServiceUriCollection;

        [ConfigurationProperty("AccountName", IsRequired = false)]
        public string AccountName
        {
            get => base[propAccountName] as string;
            set => base[propAccountName] = value;
        }

        #endregion

    }
}
