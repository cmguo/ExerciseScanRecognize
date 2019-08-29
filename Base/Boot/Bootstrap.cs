using System;

namespace Base.Boot
{
    public class Bootstrap
    {

        private static BaseBootstrapper baseBootstrapper = new BaseBootstrapper();

        private static bool isStarted = false;

        public static void Start()
        {
            if (isStarted)
                return;
            isStarted = true;
            baseBootstrapper.Run();
        }

        public static void Stop()
        {
            baseBootstrapper.Stop();
        }
    }
}
