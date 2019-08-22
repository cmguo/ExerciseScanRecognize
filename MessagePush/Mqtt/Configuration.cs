namespace MessagePush.Mqtt
{

    class Configuration : Base.Misc.Configuration
    {
        public static string Uri = GetByKey("MqttUri");

        public static string InstanceId = GetByKey("MqttInstanceId");

        public static string AccessKey = GetByKey("MqttAccessKey");

        public static string SecretKey = GetByKey("MqttSecretKey");

        public static string GroupId = GetByKey("MqttGroupId");

        public static ushort KeepAlivePeriod = ushort.Parse(GetByKey("MqttKeepAlivePeriod", "60"));

        public static string ParentTopic = GetByKey("MqttParentTopic");

        public static string ClientId
        {
            get => GetByKey("MqttClientId");
            set => SetByKey("MqttClientId", value);
        }
    }
}
