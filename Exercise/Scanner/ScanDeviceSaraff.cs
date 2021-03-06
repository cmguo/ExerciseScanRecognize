using Base.Helpers;
using Base.Misc;
using Saraff.Twain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Exercise.Scanner
{
    public partial class ScanDeviceSaraff : IScanDevice
    {
        private static readonly Logger Log = Logger.GetLogger<ScanDeviceSaraff>();

        private const int CANCEL_DROP = 1;
        private const int CANCEL_STOP = 2;


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

        public bool FeederEnabled
        {
            get => twain32.Capabilities.FeederEnabled.GetCurrent();
            set => twain32.Capabilities.FeederEnabled.Set(value);
        }

        public bool PaperDectectable => twain32.Capabilities.PaperDetectable.GetCurrent();

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
        public event EventHandler<ScanEvent> ScanEvent;
        public event EventHandler<ScanEvent> ScanException;
        public event EventHandler<ScanEvent> ScanCompleted;

        private Window window;
        private Twain32 twain32;
        private int cancel;
        private ImageDevice[] imageDevices;

        public ScanDeviceSaraff(Window window)
        {
            this.window = window;
            ImageDevice.Init(window);
            twain32 = new Twain32(window);
            twain32.Language = TwLanguage.CHINESE_SIMPLIFIED;
            twain32.Country = TwCountry.CHINA;
            twain32.ShowUI = false;
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            //twain32.AppProductName = versionInfo.ProductName;
            twain32.DeviceEvent += Twain32_DeviceEvent; ;
            twain32.SetupFileXferEvent += Twain32_SetupFileXferEvent;
            twain32.XferDone += Twain32_XferDone;
            twain32.FileXferEvent += Twain32_FileXferEvent; // 很慢
            twain32.AcquireError += Twain32_AcquireError;
            twain32.AcquireCompleted += Twain32_AcquireCompleted;
        }

        public void Open()
        {
            twain32.OpenDSM();
            imageDevices = new ImageDevice[twain32.SourcesCount];
        }

        public void DetectSource()
        {
            for (int i = 0; i < SourceList.Length; ++i)
            {
                try
                {
                    twain32.CloseDataSource();
                    twain32.SourceIndex = i;
                    twain32.OpenDataSource();
                    break;
                }
                catch
                {
                }
            }
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
            if (twain32.Capabilities.SupportedSizes.IsSupported(TwQC.Set))
                twain32.Capabilities.SupportedSizes.Set(TwSS.A4);
            if (twain32.Capabilities.Orientation.IsSupported(TwQC.Set))
                twain32.Capabilities.Orientation.Set(TwOR.Rot0);
            if (twain32.Capabilities.Rotation.IsSupported(TwQC.Set))
                twain32.Capabilities.Rotation.Set(0f);
            if (twain32.Capabilities.AutomaticRotate.IsSupported(TwQC.Set))
                twain32.Capabilities.AutomaticRotate.Set(false);
            if (twain32.Capabilities.AutoDiscardBlankPages.IsSupported(TwQC.Set))
                twain32.Capabilities.AutoDiscardBlankPages.Set(TwBP.Disable);
            if (twain32.Capabilities.DeviceEvent.IsSupported(TwQC.Set))
            {
                twain32.Capabilities.DeviceEvent.Set(TwDE.DeviceOffline);
                twain32.Capabilities.DeviceEvent.Set(TwDE.CheckDeviceOnline);
                twain32.Capabilities.DeviceEvent.Set(TwDE.PaperJam);
                twain32.Capabilities.DeviceEvent.Set(TwDE.PaperDoubleFeed);
            }
            if (twain32.Capabilities.CameraSide.IsSupported(TwQC.Set)) // AT440
                twain32.Capabilities.CameraSide.Set(TwCS.Both);
            twain32.Capabilities.DuplexEnabled.Set(true);
            twain32.Capabilities.ImageFileFormat.Set(TwFF.Jfif);
            twain32.Capabilities.Compression.Set(TwCompression.Jpeg);
            twain32.Capabilities.XferMech.Set(TwSX.File);
            twain32.Capabilities.PixelType.Set(TwPixelType.RGB);
            XResolution = 200;
            YResolution = 200;
            //AttachImageDevice();
        }

        public void CheckStatus()
        {
            //TwCC condition = twain32._GetTwainStatus();
            //if (condition != TwCC.Success)
            //    throw new Exception(condition.ToString());
            OpenDataSource();
            //if (imageDevices[SourceIndex] != null && !imageDevices[SourceIndex].Present)
            //    throw new InvalidOperationException("image device not connected");
        }

        public void Scan(short count)
        {
            lock (twain32)
            {
                cancel = 0;
            }
            twain32.Capabilities.XferCount.Set(count <= 0 ? (short) -1 : (short) (count * 2));
            twain32.Acquire();
        }


        public void CancelScan(bool drop)
        {
            lock (twain32)
            {
                cancel = drop ? CANCEL_DROP : CANCEL_STOP;
            }
        }

        public void Close()
        {
            twain32.CloseDataSource();
        }

        private void AttachImageDevice()
        {
            string scanner = twain32.GetSourceProductName(SourceIndex);
            if (imageDevices[SourceIndex] == null)
            {
                ImageDevice device = ImageDevice.List.MinItem(
                    d => EditDistance(d.Caption, scanner));
                imageDevices[SourceIndex] = device;
            }
        }

        private int EditDistance(string word1, string word2)
        {
            int[] v1 = new int[word2.Length + 1];
            int[] v2 = new int[word2.Length + 1];
            for (int i = 1; i <= word2.Length; ++i)
                v2[i] = i;
            for (int i = 0; i < word1.Length; ++i)
            {
                int[] vo = (i % 2) != 0 ? v1 : v2;
                int[] vn = (i % 2) != 0 ? v2 : v1;
                for (int j = 0; j < word2.Length; ++j)
                {
                    if (word1[i] == word2[j])
                        vn[j + 1] = vo[j];
                    else
                        vn[j + 1] = Math.Min(vo[j], Math.Min(vn[j], vo[j + 1])) + 1;
                }
            }
            int[] v = (word1.Length % 2) == 1 ? v1 : v2;
            return v[word2.Length];
        }

        private bool CheckStatus(Twain32.SerializableCancelEventArgs e)
        {
            lock (twain32)
            {
                e.Cancel = cancel > 0;
                e.Drop = cancel == CANCEL_DROP;
                return cancel == CANCEL_DROP;
            }

        }

        private void Twain32_DeviceEvent(object sender, Twain32.DeviceEventEventArgs e)
        {
            Log.d("Twain32_DeviceEvent: " + e.Event);
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
                e.FileName = e1.FileName;
            }
        }

        private void Twain32_XferDone(object sender, Twain32.XferDoneEventArgs e)
        {
            Log.d("Twain32_XferDone");
            //CheckStatus(e);
        }

        private void Twain32_FileXferEvent(object sender, Twain32.FileXferEventArgs e)
        {
            Log.d("Twain32_FileXferEvent: " + e.ImageFileXfer.FileName);
            CheckStatus(e);
            if (OnImage != null)
            {
                window.Dispatcher.Invoke(() =>
                    OnImage(this, new ScanEvent() { FileName = e.ImageFileXfer.FileName }));
            }
        }

        private void Twain32_AcquireCompleted(object sender, EventArgs e)
        {
            Log.d("Twain32_AcquireCompleted");
            if (ScanCompleted != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanCompleted(this, new ScanEvent()));
            }
        }

        private void Twain32_AcquireError(object sender, Twain32.AcquireErrorEventArgs e)
        {
            Log.w("Twain32_AcquireError ", e.Exception);
            if (ScanException != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanException(this, new ScanEvent() { Exception = e.Exception }));
            }
            if (ScanCompleted != null)
            {
                window.Dispatcher.Invoke(() =>
                    ScanCompleted(this, new ScanEvent()));
            }
        }

    }
}
