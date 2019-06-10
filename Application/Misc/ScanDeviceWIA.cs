using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using WIA;

namespace Application.Misc
{
    class ScanDeviceWIA
    {
        private const string WIAFormatPNG = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}";

        private List<DeviceInfo> Scaners;
        private DeviceInfo SelectedDevice;
        private Device ConntectedScaner;

        public ScanDeviceWIA()
        {
            DeviceManager manager = new DeviceManager();
            Scaners = new List<DeviceInfo>();
            foreach (DeviceInfo info in manager.DeviceInfos)
            {
                if (info.Type != WiaDeviceType.ScannerDeviceType) continue;
                Scaners.Add(info);
                break;
            }
        }

        public void Select()
        {
            if (SelectedDevice == null)
            {
                if (Scaners.Count > 0)
                    SelectedDevice = Scaners[0];
            }
        }

        public bool Connect()
        {
            
            ConntectedScaner = SelectedDevice.Connect();
            return ConntectedScaner != null;
        }

        public async Task<List<ImageSource>> Scan()
        {
            if (ConntectedScaner == null)
            {
                //ImageFile imageFile = ConntectedScaner.Items[1].Transfer(WIAFormatPNG);
                //return imageFile;
            }
            return null;
        }

    }

}
