using Base.Config;
using System.Configuration;

namespace MessagePush.Mqtt
{

    public class M2MqttConfig : BaseSection
    {

        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty propServiceUri;
        private static ConfigurationProperty propInstanceId;
        private static ConfigurationProperty propGroupId;
        private static ConfigurationProperty propAccessKey;
        private static ConfigurationProperty propSecretKey;
        private static ConfigurationProperty propParentTopic;
        private static ConfigurationProperty propKeepAlivePeriod;
        private static ConfigurationProperty propClientId;

        static M2MqttConfig()
        {
            propServiceUri = new ConfigurationProperty("ServiceUri", typeof(string),
                null, ConfigurationPropertyOptions.IsRequired);
            propInstanceId = new ConfigurationProperty("InstanceId", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propGroupId = new ConfigurationProperty("GroupId", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propAccessKey = new ConfigurationProperty("AccessKey", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propSecretKey = new ConfigurationProperty("SecretKey", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propGroupId = new ConfigurationProperty("GroupId", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propParentTopic = new ConfigurationProperty("ParentTopic", typeof(string),
                null, ConfigurationPropertyOptions.None);
            propKeepAlivePeriod = new ConfigurationProperty("KeepAlivePeriod", typeof(ushort),
                (ushort)60, ConfigurationPropertyOptions.None);
            propClientId = new ConfigurationProperty("ClientId", typeof(string),
                null, ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propServiceUri);
            properties.Add(propInstanceId);
            properties.Add(propGroupId);
            properties.Add(propAccessKey);
            properties.Add(propSecretKey);
            properties.Add(propGroupId);
            properties.Add(propParentTopic);
            properties.Add(propKeepAlivePeriod);
            properties.Add(propClientId);
        }

        public static M2MqttConfig Instance => ConfigurationManager.GetSection("m2mqtt") as M2MqttConfig;

        #region Properties

        protected override ConfigurationPropertyCollection Properties => properties;

        [ConfigurationProperty("ServiceUri", IsRequired = true)]
        public string ServiceUri
        {
            get => base[propServiceUri] as string;
            set => base[propServiceUri] = value;
        }

        [ConfigurationProperty("InstanceId")]
        public string InstanceId => base[propInstanceId] as string;

        [ConfigurationProperty("propGroupId")]
        public string GroupId => base[propGroupId] as string;

        [ConfigurationProperty("AccessKey")]
        public string AccessKey => base[propAccessKey] as string;

        [ConfigurationProperty("SecretKey")]
        public string SecretKey => base[propSecretKey] as string;

        [ConfigurationProperty("ParentTopic")]
        public string ParentTopic => base[propParentTopic] as string;

        [ConfigurationProperty("KeepAlivePeriod")]
        public ushort KeepAlivePeriod => (ushort)base[propKeepAlivePeriod];

        [ConfigurationProperty("ClientId")]
        public string ClientId
        {
            get => base[propClientId] as string;
            set => base[propClientId] = value;
        }

        #endregion

    }
}
