using Exercise;
using Saraff.Twain;
using System;

namespace Application.Misc
{
    public partial class ScanDeviceSaraff : IScanDevice
    {

        public static void init()
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

        public event EventHandler<ImageEvent> OnImage;
        public event EventHandler<ImageEvent> GetFileName;
        public event EventHandler<ImageEvent> ScanCompleted;

        private Twain32 twain32;
        private bool cancel;

        public ScanDeviceSaraff()
        {
            twain32 = new Twain32();
            twain32.Language = TwLanguage.CHINESE_SIMPLIFIED;
            twain32.Country = TwCountry.CHINA;
            twain32.SetupFileXferEvent += Twain32_SetupFileXferEvent; ;
            twain32.FileXferEvent += Twain32_FileXferEvent;
        }

        public void Open()
        {
            twain32.OpenDSM();
            twain32.OpenDataSource();
            twain32.Capabilities.XferMech.Set(TwSX.MemFile);
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
                ImageEvent e1 = new ImageEvent();
                GetFileName.Invoke(this, e1);
                e.FileName = e1.FileName;
            }
        }

        private void Twain32_FileXferEvent(object sender, Twain32.FileXferEventArgs e)
        {
            if (OnImage != null)
            {
                OnImage(this, new ImageEvent() { FileName = e.ImageFileXfer.FileName });
            }
        }


    }
}
