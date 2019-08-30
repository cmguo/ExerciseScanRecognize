using log4net.Appender;
using log4net.DateFormatter;
using System;
using System.IO;
using System.Text;

namespace Base.Misc
{
    public class FileAppender : RollingFileAppender
    {
        public FileAppender()
        {
            Encoding = Encoding.UTF8;
            AppendToFile = true;
            PreserveLogFileNameExtension = true;
            StaticLogFileName = true;
            MaxFileSize = 2 * 1024 * 1024;
            DatePattern = @"'Logs\\'yyyy.MM.dd";
            RollingStyle = RollingMode.Composite;
            MaxSizeRollBackups = 100;
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            string logs = File.Remove(File.LastIndexOf('.')) + "Logs";
            Directory.CreateDirectory(logs);
        }

    }
}
