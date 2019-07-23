using System;
using System.IO;

namespace Exercise
{
    public class Component
    {
        private static readonly string ROOT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static readonly string DATA_PATH = ROOT_PATH + "\\Exercise";

        static Component()
        {
            Directory.CreateDirectory(DATA_PATH);
        }
    }
}
