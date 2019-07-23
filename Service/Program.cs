using Base.Misc;
using Exercise.Algorithm;
using System;
using System.Diagnostics;

namespace Service
{
    class Program
    {

        private static readonly Logger Log = Logger.GetLogger<Program>();

        static void Main(string[] args)
        {
            Logger.SetLogPath(Exercise.Component.DATA_PATH);
            string procName = Process.GetCurrentProcess().MainModule.FileName;
            Logger.Config(new Uri(procName.Replace("\\Service.exe", "\\servicelogger.xml")));
            Jni.Init();
            new Algorithm(false).ServiceMain(args);
        }

    }
}
