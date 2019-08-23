namespace Base.Boot
{
    public class Bootstrap
    {

        private static BaseBootstrapper baseBootstrapper = new BaseBootstrapper();

        public static void Start()
        {
            baseBootstrapper.Run();
        }

        public static void Stop()
        {
            baseBootstrapper.Stop();
        }
    }
}
