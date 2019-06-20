using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise
{
    public class ScanEvent
    {
        public string FileName { get; set; }
    }

    public interface IScanDevice
    {

        event EventHandler<ScanEvent> OnImage;

        event EventHandler<ScanEvent> GetFileName;

        event EventHandler<ScanEvent> ScanCompleted;

        string[] SourceList { get; }

        int SourceIndex { get; set; }

        bool DuplexEnabled { get; set; }

        string ImageFormat { get; set; }

        float XResolution { get; set; }
        float YResolution { get; set; }

        bool PaperDetectable { get; }

        void Open();

        void Scan(short count);

        void CancelScan();

        void PauseScan();

        void ResumeScan();

        void Close();
    }

    public class ScanDevice
    {
        public static IScanDevice Instance { get; set; }
    }

}
