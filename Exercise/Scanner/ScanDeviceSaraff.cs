using Exercise;
using Saraff.Twain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Exercise.Scanning
{
    public partial class ScanDeviceSaraff : IScanDevice
    {

        public bool Indicators
        {
            get => twain32.Capabilities.Indicators.GetCurrent();
            set => twain32.Capabilities.Indicators.Set(value);
        }

        public string[] SourceList
        {
            get
            {
                string[] names = new string[twain32.SourcesCount];
                for (int i = 0; i < names.Length; ++i)
                    names[i] = twain32.GetSourceProductName(i);
                return names;
            }
        }

        public int SourceIndex
        {
            get
            {
                int index = twain32.SourceIndex;
                if (index < 0)
                {
                    index = twain32.GetDefaultSource();
                }
                return index;
            }
            set
            {
                if (SourceIndex == value)
                {
                    OpenDataSource();
                    return;
                }
                twain32.CloseDataSource();
                twain32.SourceIndex = value;
                OpenDataSource();
            }
        }

        public string CameraSide
        {
            get => twain32.Capabilities.CameraSide.GetCurrent().ToString();
            set => twain32.Capabilities.CameraSide.Set((TwCS)Enum.Parse(typeof(TwCS), value));
        }

        public string Duplex
        {
            get => twain32.Capabilities.Duplex.GetCurrent().ToString();
            set => twain32.Capabilities.Duplex.Set((TwDX)Enum.Parse(typeof(TwDX), value));
        }

        public bool DuplexEnabled
        {
            get => twain32.Capabilities.DuplexEnabled.GetCurrent();
            set => twain32.Capabilities.DuplexEnabled.Set(value);
        }

        public string ImageFormat
        {
            get => twain32.Capabilities.ImageFileFormat.GetCurrent().ToString();
            set => twain32.Capabilities.ImageFileFormat.Set((TwFF)Enum.Parse(typeof(TwFF), value));
        }

        public float XResolution
        {
            get => twain32.Capabilities.XResolution.GetCurrent();
            set => twain32.Capabilities.XResolution.Set(value);
        }

        public float YResolution
        {
            get => twain32.Capabilities.YResolution.GetCurrent();
            set => twain32.Capabilities.YResolution.Set(value);
        }

        public string PixelType
        {
            get => twain32.Capabilities.PixelType.GetCurrent().ToString();
            set => twain32.Capabilities.PixelType.Set((TwPixelType)Enum.Parse(typeof(TwPixelType), value));
        }

        public bool FeederLoaded => twain32.Capabilities.FeederLoaded.GetCurrent();

        public ICollection<string> ImageFormats
        {
            get {
                Twain32.Enumeration list = twain32.Capabilities.ImageFileFormat.Get();
                string[] vec = new string[list.Count];
                for (int i = 0; i < vec.Length; ++i)
                    vec[i] = list[i].ToString();
                return vec;
            }
        }

        public event EventHandler<ScanEvent> OnImage;
        public event EventHandler<ScanEvent> GetFileName;
        public event EventHandler<ScanEvent> ScanPaused;
        public event EventHandler<ScanEvent> ScanError;
        public event EventHandler<ScanEvent> ScanCompleted;

        private Window window;
        private Twain32 twain32;
        private bool cancel;
        private bool pause;

        public ScanDeviceSaraff(Window window)
        {
            this.window = window;
            twain32 = new Twain32(window);
            twain32.Language = TwLanguage.CHINESE_SIMPLIFIED;
            twain32.Country = TwCountry.CHINA;
            twain32.ShowUI = false;
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            //twain32.AppProductName = versionInfo.ProductName;
            twain32.SetupFileXferEvent += Twain32_SetupFileXferEvent;
            twain32.XferDone += Twain32_XferDone;
            twain32.FileXferEvent += Twain32_FileXferEvent; // 很慢
            twain32.AcquireError += Twain32_AcquireError;
            twain32.AcquireCompleted += Twain32_AcquireCompleted;
        }

        public void Open()
        {
             twain32.OpenDSM();
        }

        public void OpenDataSource()
        {
            //twain32.SelectSource();
            twain32.OpenDataSource();
            //twain32.Capabilities.DeviceOnline.Get();
            twain32.Capabilities.Indicators.Set(false);
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            if (twain32.Capabilities.Author.IsSupported(TwQC.Set))
                twain32.Capabilities.Author.Set(versionInfo.CompanyName);
            if (twain32.Capabilities.Caption.IsSupported(TwQC.Set))
                twain32.Capabilities.Caption.Set(versionInfo.ProductName);
            twain32.Capabilities.CameraSide.Set(TwCS.Both);
            if (twain32.Capabilities.AutoDiscardBlankPages.IsSupported(TwQC.Set))
                twain32.Capabilities.AutoDiscardBlankPages.Set(TwBP.Disable);
            twain32.Capabilities.DuplexEnabled.Set(true);
            twain32.Capabilities.ImageFileFormat.Set(TwFF.Jfif);
            twain32.Capabilities.Compression.Set(TwCompression.Jpeg);
            twain32.Capabilities.XferMech.Set(TwSX.File);
            twain32.Capabilities.PixelType.Set(TwPixelType.RGB);
            XResolution = 200;
            YResolution = 200;
        }

        public void Scan(short count)
        {
            lock (twain32)
            {
                cancel = false;
                pause = false;
            }
            twain32.Capabilities.XferCount.Set(count <= 0 ? (short) -1 : (short) (count * 2));
            twain32.Acquire();
        }


        public void CancelScan()
        {
            lock (twain32)
            {
                cancel = true;
                pause = false;
                Monitor.PulseAll(twain32);
            }
        }

        public void PauseScan()
        {
            lock (twain32)
            {
                pause = true;
            }
        }

        public void ResumeScan()
        {
            lock (twain32)
            {
                pause = false;
                Monitor.PulseAll(twain32);
            }
        }

        public void Close()
        {
            twain32.CloseDataSource();
        }

        private bool CheckStatus(Twain32.SerializableCancelEventArgs e)
        {
            lock (twain32)
            {
                while (pause)
                {
                    ScanPaused?.Invoke(this, new ScanEvent());
                    Monitor.Wait(twain32);
                }
                e.Cancel = cancel;
                return cancel;
            }

        }

        private void Twain32_SetupFileXferEvent(object sender, Twain32.SetupFileXferEventArgs e)
        {
            if (CheckStatus(e))
            {
                return;
            }
            if (GetFileName != null)
            {
                ScanEvent e1 = new ScanEvent();
                GetFileName.Invoke(this, e1);
                Debug.WriteLine("Twain32_SetupFileXferEvent: " + e1.FileName);
                e.FileName = e1.FileName;
            }
        }

        private void Twain32_XferDone(object sender, Twain32.XferDoneEventArgs e)
        {
            Debug.WriteLine("Twain32_XferDone");
            CheckStatus(e);
        }

        private void Twain32_FileXferEvent(object sender, Twain32.FileXferEventArgs e)
        {
            Debug.WriteLine("Twain32_FileXferEvent: " + e.ImageFileXfer.FileName);
            if (OnImage != null)
            {
                window.Dispatcher.Invoke(() =>
                    OnImage(this, new ScanEvent() { FileName = e.ImageFileXfer.FileName }));
            }
        }

        private void Twain32_AcquireCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("Twain32_AcquireCompleted");
            if (ScanCompleted != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanCompleted(this, new ScanEvent()));
            }
        }

        private void Twain32_AcquireError(object sender, Twain32.AcquireErrorEventArgs e)
        {
            Debug.WriteLine("Twain32_AcquireError " + e.Exception.Message);
            if (ScanError != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanError(this, new ScanEvent() { Error = e.Exception }));
            }
            if (ScanCompleted != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanCompleted(this, new ScanEvent()));
            }
        }

    }
}
