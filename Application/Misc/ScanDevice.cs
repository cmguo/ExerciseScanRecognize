using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Application.Misc
{

    interface IScanDevice
    {
        void Select();
        bool Connect();
        Task<List<ImageSource>> Scan();
        void Close();
    }

    class ScanDevice
    {
        private IScanDevice Device;

        public ScanDevice()
        {
            Device = new ScanDeviceDSM();
        }

        public void Select()
        {
            Device.Select();
        }

        public bool Connect()
        {
            return Device.Connect();
        }

        public async Task<List<ImageSource>> Scan()
        {
            return await Device.Scan();
        }

        public void Close()
        {
            Device.Close();
            Device = null;
        }

    }

}
