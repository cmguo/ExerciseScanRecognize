using System;

namespace Exercise.Scanning
{
    public class ScanEvent : EventArgs
    {
        public string FileName { get; set; }
        public Exception Error { get; set; }
    }

    public interface IScanDevice
    {

        #region Events

        event EventHandler<ScanEvent> OnImage;

        event EventHandler<ScanEvent> GetFileName;

        event EventHandler<ScanEvent> ScanPaused;

        event EventHandler<ScanEvent> ScanError;

        event EventHandler<ScanEvent> ScanEvent;

        event EventHandler<ScanEvent> ScanCompleted;

        #endregion

        #region Properties

        string[] SourceList { get; }

        int SourceIndex { get; set; }

        bool DuplexEnabled { get; set; }

        string ImageFormat { get; set; }

        float XResolution { get; set; }
        float YResolution { get; set; }

        bool FeederEnabled { get; set; }

        bool PaperDectectable { get; }

        bool FeederLoaded { get; }

        #endregion

        #region Methods

        void Open();

        void DetectSource();

        void CheckStatus();

        void Scan(short count);

        void CancelScan();

        void PauseScan();

        void ResumeScan();

        void Close();

        #endregion

    }

}
