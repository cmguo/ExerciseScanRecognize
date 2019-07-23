using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace Base.Misc
{
    public class Logger
    {

        public enum Level
        {
            ASSERT, 
            ERROR, 
            WARN,
            DEBUG, 
            INFO, 
            VERBOSE,
        }

        public static void SetLogPath(string path)
        {
            GlobalContext.Properties["LogPath"] = path;
        }

        public static void Config(string file)
        {
            StreamResourceInfo resource = 
                Application.GetContentStream(new Uri(file, UriKind.Relative));
            if (resource != null)
                XmlConfigurator.Configure(resource.Stream);
        }

        public static void Config(Uri uri)
        {
            XmlConfigurator.Configure(uri);
        }

        public static Logger GetLogger(string name)
        {
            return new Logger(name);
        }

        public static Logger GetLogger<T>()
        {
            return new Logger(typeof(T));
        }

        private log4net.ILog log;

        public Logger(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public Logger(Type type)
        {
            log = LogManager.GetLogger(type);
        }

        /*
         * assert
         */

        public void wtf(String msg)
        {
            wtf(msg, null, null);
        }

        public void wtf(String msg, Exception tr)
        {
            wtf(msg, null, tr);
        }

        public void wtf(Object msg)
        {
            wtf(ToString(msg), null, null);
        }

        public void wtf(Object msg, Exception tr)
        {
            wtf(ToString(msg), null, tr);
        }

        public void wtf(Exception tr)
        {
            wtf(tr.Message, null, tr);
        }

        public void wtf(String msg, Object obj)
        {
            wtf(msg, obj, null);
        }

        public void wtf(String msg, Object obj, Exception tr)
        {
            log.Fatal(msg, tr);
        }

        /*
         * error
         */

        public void e(String msg)
        {
            e(msg, null, null);
        }

        public void e(String msg, Exception tr)
        {
            e(msg, null, tr);
        }

        public void e(Object msg)
        {
            e(ToString(msg), null, null);
        }

        public void e(Object msg, Exception tr)
        {
            e(ToString(msg), null, tr);
        }

        public void e(Exception tr)
        {
            e(tr.Message, null, tr);
        }

        public void e(String msg, Object obj)
        {
            e(msg, obj, null);
        }

        public void e(String msg, Object obj, Exception tr)
        {
            log.Error(msg, tr);
        }

        /*
         * waring
         */

        public void w(String msg)
        {
            w(msg, null, null);
        }

        public void w(String msg, Exception tr)
        {
            w(msg, null, tr);
        }

        public void w(Object msg)
        {
            w(ToString(msg), null, null);
        }

        public void w(Object msg, Exception tr)
        {
            w(ToString(msg), null, tr);
        }

        public void w(Exception tr)
        {
            w(tr.Message, null, tr);
        }

        public void w(String msg, Object obj)
        {
            w(msg, obj, null);
        }

        public void w(String msg, Object obj, Exception tr)
        {
            log.Warn(msg, tr);
        }

        /*
         * info
         */

        public void i(String msg)
        {
            i(msg, null, null);
        }

        public void i(String msg, Exception tr)
        {
            i(msg, null, tr);
        }

        public void i(Object msg)
        {
            i(ToString(msg), null, null);
        }

        public void i(Object msg, Exception tr)
        {
            i(ToString(msg), null, tr);
        }

        public void i(Exception tr)
        {
            i(tr.Message, null, tr);
        }

        public void i(String msg, Object obj)
        {
            i(msg, obj, null);
        }

        public void i(String msg, Object obj, Exception tr)
        {
            log.Info(msg, tr);
        }

        /*
         * debug
         */

        public void d(String msg)
        {
            d(msg, null, null);
        }

        public void d(String msg, Exception tr)
        {
            d(msg, null, tr);
        }

        public void d(Object msg)
        {
            d(ToString(msg), null, null);
        }

        public void d(Object msg, Exception tr)
        {
            d(ToString(msg), null, tr);
        }

        public void d(Exception tr)
        {
            d(tr.Message, null, tr);
        }

        public void d(String msg, Object obj)
        {
            d(msg, obj, null);
        }

        public void d(String msg, Object obj, Exception tr)
        {
            log.Debug(msg, tr);
        }

        /*
         * verbose
         */

        public void v(String msg)
        {
            v(msg, null, null);
        }

        public void v(String msg, Exception tr)
        {
            v(msg, null, tr);
        }

        public void v(Object msg)
        {
            v(ToString(msg), null, null);
        }

        public void v(Object msg, Exception tr)
        {
            v(ToString(msg), null, tr);
        }

        public void v(Exception tr)
        {
            v(tr.Message, null, tr);
        }

        public void v(String msg, Object obj)
        {
            v(msg, obj, null);
        }

        public void v(String msg, Object obj, Exception tr)
        {
            log.Debug(msg, tr);
        }

        private static string ToString(object msg)
        {
            return msg == null ? "null" : msg.ToString();
        }

    }
}
