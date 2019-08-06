using System.Collections.Generic;
using System.Management;
using System.Windows;

namespace Exercise.Scanner
{
    public class ImageDevice
    {
        private const string GUID_IMAGE = "{6bdd1fc6-810f-11d0-bec7-08002be2092f}";

        public static IList<ImageDevice> List => GetPNPDevices(GUID_IMAGE);

        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string ClassGuid { get; private set; }
        public string PNPClass { get; private set; }
        public bool Present => IsPresent();
        public string Description { get; private set; }

        public static void Init(Window window)
        {
        }

        private bool IsPresent()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_PnPEntity Where Name=\"" + Name + "\""))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                if ((bool)device.GetPropertyValue("Present"))
                    return true;
            }

            return false;
        }

        private static List<ImageDevice> GetPNPDevices(string clazz)
        {
            List<ImageDevice> devices = new List<ImageDevice>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_PnPEntity Where ClassGuid=\"" + clazz + "\""))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new ImageDevice()
                {
                    DeviceID = (string)device.GetPropertyValue("DeviceID"),
                    PnpDeviceID = (string)device.GetPropertyValue("PNPDeviceID"),
                    Name = (string)device.GetPropertyValue("Name"),
                    Caption = (string)device.GetPropertyValue("Caption"),
                    ClassGuid = (string)device.GetPropertyValue("ClassGuid"),
                    PNPClass = (string)device.GetPropertyValue("PNPClass"),
                    Description = (string)device.GetPropertyValue("Description")
                });
            }

            collection.Dispose();
            return devices;
        }
    }

}
