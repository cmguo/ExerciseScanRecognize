namespace Exercise
{

    class Configuration : Base.Misc.Configuration
    {
        public static string StartupWindow = GetByKey("SavePath");

        public static string SavePath
        {
            get => GetByKey("SavePath");
            set => SetByKey("SavePath", value);
        }

    }
}
