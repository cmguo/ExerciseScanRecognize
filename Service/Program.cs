﻿using Base.Misc;
using Exercise.Algorithm;
using System;
using System.Diagnostics;
using System.IO;

namespace Service
{
    class Program
    {

        private static readonly Logger Log = Logger.GetLogger<Program>();

        static void Main(string[] args)
        {
            Assistant.Fault.CrashHandler.Init(Exercise.Component.DATA_PATH);
            Logger.SetLogPath(Exercise.Component.DATA_PATH);
            string procName = Process.GetCurrentProcess().MainModule.FileName;
            Logger.Config(new Uri(procName.Replace("\\Service.exe", "\\servicelogger.xml")));
            new Algorithm(false, null).ServiceMain(args);
        }

    }
}
