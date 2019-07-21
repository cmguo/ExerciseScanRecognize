namespace Account
{

    class Configuration : Base.Misc.Configuration
    {
        public static string StartupWindow = GetByKey("StartupWindow");

        public static string AccountName
        {
            get => GetByKey("AccountName");
            set => SetByKey("AccountName", value);
        }

        public static string ServiceUri
        {
            get => GetByKey("ServiceUri");
            set => SetByKey("ServiceUri", value);
        }
    }
}
