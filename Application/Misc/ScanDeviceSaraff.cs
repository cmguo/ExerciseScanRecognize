using Exercise;
using Saraff.Twain;
using System;

namespace Application.Misc
{
    public partial class ScanDeviceSaraff : IScanDevice
    {

        public static void Init()
        {
            Exercise.ScanDevice.Instance = new ScanDeviceSaraff();
        }

        public bool Duplex
        {
            get => twain32.Capabilities.DuplexEnabled.GetCurrent();
            set => twain32.Capabilities.DuplexEnabled.Set(value);
        }
        public string ImageFormat
        {
            get => twain32.Capabilities.ImageFileFormat.GetCurrent().ToString();
            set => twain32.Capabilities.ImageFileFormat.Set((TwFF) Enum.Parse(typeof(TwFF), value));
        }

        public event EventHandler<ScanEvent> OnImage;
        public event EventHandler<ScanEvent> GetFileName;
        public event EventHandler<ScanEvent> ScanCompleted;

        private Twain32 twain32;
        private bool cancel;

        public ScanDeviceSaraff()
        {
            twain32 = new Twain32();
            twain32.Language = TwLanguage.CHINESE_SIMPLIFIED;
            twain32.Country = TwCountry.CHINA;
            twain32.ShowUI = false;
            twain32.SetupFileXferEvent += Twain32_SetupFileXferEvent;
            twain32.FileXferEvent += Twain32_FileXferEvent;
            twain32.XferDone += Twain32_XferDone;
            twain32.EndXfer += Twain32_EndXfer;
            twain32.AcquireError += Twain32_AcquireError;
            twain32.AcquireCompleted += Twain32_AcquireCompleted;
        }

        public void Open()
        {
            twain32.OpenDSM();
            twain32.OpenDataSource();
            twain32.Capabilities.Indicators.Set(false);
            twain32.Capabilities.XferMech.Set(TwSX.File);
        }

        public void Scan(short count)
        {
            twain32.Capabilities.XferCount.Set(count);
            twain32.Acquire();
        }

        
        public void CancelScan()
        {
            cancel = true;
        }

        
        public void Close()
        {
            twain32.CloseDataSource();
        }

        private void Twain32_SetupFileXferEvent(object sender, Twain32.SetupFileXferEventArgs e)
        {
            if (cancel)
            {
                e.Cancel = true;
                return;
            }
            if (GetFileName != null)
            {
                ScanEvent e1 = new ScanEvent();
                GetFileName.Invoke(this, e1);
                e.FileName = e1.FileName;
            }
        }

        private void Twain32_FileXferEvent(object sender, Twain32.FileXferEventArgs e)
        {
            if (OnImage != null)
            {
                OnImage(this, new ScanEvent() { FileName = e.ImageFileXfer.FileName });
            }
        }

        private void Twain32_XferDone(object sender, Twain32.XferDoneEventArgs e)
        {
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

        private void Twain32_EndXfer(object sender, Twain32.EndXferEventArgs e)
        {
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

        private void Twain32_AcquireCompleted(object sender, EventArgs e)
        {
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

        private void Twain32_AcquireError(object sender, Twain32.AcquireErrorEventArgs e)
        {
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

    }
}
