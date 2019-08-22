using Base.Misc;
using Prism.Logging;

namespace Base.Boot
{
    class LoggerFacade : ILoggerFacade
    {
        private Logger Logger = Logger.GetLogger("Base");

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Logger.d(message);
                    break;
                case Category.Warn:
                    Logger.w(message);
                    break;
                case Category.Info:
                    Logger.i(message);
                    break;
                case Category.Exception:
                    Logger.e(message);
                    break;
            }
        }
    }
}
