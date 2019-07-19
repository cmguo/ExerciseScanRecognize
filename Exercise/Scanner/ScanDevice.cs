using System;

namespace Exercise.Scanning
{
    public class ScanEvent
    {
        public string FileName { get; set; }
        public Exception Error { get; set; }
    }

    public interface IScanDevice
    {

        event EventHandler<ScanEvent> OnImage;

        event EventHandler<ScanEvent> GetFileName;

        event EventHandler<ScanEvent> ScanPaused;

        event EventHandler<ScanEvent> ScanError;

        event EventHandler<ScanEvent> ScanCompleted;

        string[] SourceList { get; }

        int SourceIndex { get; set; }

        bool DuplexEnabled { get; set; }

        string ImageFormat { get; set; }

        float XResolution { get; set; }
        float YResolution { get; set; }

        bool FeederLoaded { get; }

        void Open();

        void DetectSource();

        void Scan(short count);

        void CancelScan();

        void PauseScan();

        void ResumeScan();

        void Close();
    }

}
