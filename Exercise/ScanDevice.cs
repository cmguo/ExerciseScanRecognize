using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise
{
    public class ImageEvent
    {
        public string FileName { get; set; }
    }

    public interface IScanDevice
    {

        event EventHandler<ImageEvent> OnImage;

        event EventHandler<ImageEvent> GetFileName;

        event EventHandler<ImageEvent> ScanCompleted;

        bool Duplex { get; set; }

        string ImageFormat { get; set; }

        void Open();

        void Scan(short count);

        void CancelScan();

        void Close();
    }

    public class ScanDevice
    {
        public static IScanDevice Instance { get; set; }
    }

}
