using Base.Boot;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Exercise
{
    [Export(typeof(IProduct))]
    public class Component : IProduct
    {

        public static readonly string PRODUCT_CODE = "{CEE6B7E5-CBD9-4524-A6D4-315C73084322}";

        public static readonly string DATA_PATH = GetRootPath() + "\\Exercise";

        public string ProductCode => PRODUCT_CODE;

        public string LogPath => DATA_PATH;

        static Component()
        {
            Directory.CreateDirectory(DATA_PATH);
        }

        private static string GetRootPath()
        {
            string path = ExerciseConfig.Instance.SavePath;
            if (path != null)
                return path;
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            foreach (char c in path)
            {
                if (c > 0x7f)
                {
                    path = null;
                    break;
                }
            }
            if (path == null)
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                DriveInfo candidate = null;
                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveType != DriveType.Fixed)
                        continue;
                    if (candidate == null || d.TotalFreeSpace > candidate.TotalFreeSpace)
                        candidate = d;
                }
                path = candidate.RootDirectory.FullName;
            }
            return path;
        }

    }
}
