using System;

namespace Exercise.Scanning
{
    public class ScanEvent : EventArgs
    {
        public string FileName { get; set; }
        public Exception Exception { get; set; }
    }

    public interface IScanDevice
    {

        #region Events

        event EventHandler<ScanEvent> OnImage;

        event EventHandler<ScanEvent> GetFileName;

        event EventHandler<ScanEvent> ScanException;

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

        void CancelScan(bool drop);

        void Close();

        #endregion

    }

}
