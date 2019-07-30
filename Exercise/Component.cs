using System;
using System.IO;

namespace Exercise
{
    public class Component
    {
        public static readonly string DATA_PATH = GetRootPath() + "\\Exercise";

        static Component()
        {
            Directory.CreateDirectory(DATA_PATH);
        }

        private static string GetRootPath()
        {
            string path = Configuration.SavePath;
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
