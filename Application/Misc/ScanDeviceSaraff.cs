using Exercise;
using Saraff.Twain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Application.Misc
{
    public partial class ScanDeviceSaraff : IScanDevice
    {

        public static void Init(Window window)
        {
            Exercise.ScanDevice.Instance = new ScanDeviceSaraff(window);
        }

        public bool Duplex
        {
            get => twain32.Capabilities.DuplexEnabled.GetCurrent();
            set => twain32.Capabilities.DuplexEnabled.Set(value);
        }
        public string ImageFormat
        {
            get => twain32.Capabilities.ImageFileFormat.GetCurrent().ToString();
            set => twain32.Capabilities.ImageFileFormat.Set((TwFF)Enum.Parse(typeof(TwFF), value));
        }

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
        public event EventHandler<ScanEvent> ScanCompleted;

        private Twain32 twain32;
        private bool cancel;
        private bool pause;

        public ScanDeviceSaraff(Window window)
        {
            twain32 = new Twain32(window);
            twain32.Language = TwLanguage.CHINESE_SIMPLIFIED;
            twain32.Country = TwCountry.CHINA;
            twain32.ShowUI = false;
            twain32.SetupFileXferEvent += Twain32_SetupFileXferEvent;
            twain32.XferDone += Twain32_XferDone;
            twain32.FileXferEvent += Twain32_FileXferEvent; // 很慢
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
            lock (twain32)
            {
                cancel = false;
                pause = false;
            }
            twain32.Capabilities.XferCount.Set(count);
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
                Console.Out.WriteLine("Twain32_SetupFileXferEvent: " + e1.FileName);
                e.FileName = e1.FileName;
            }
        }

        private void Twain32_FileXferEvent(object sender, Twain32.FileXferEventArgs e)
        {
            Console.Out.WriteLine("Twain32_FileXferEvent: " + e.ImageFileXfer.FileName);
            if (OnImage != null)
            {
                OnImage(this, new ScanEvent() { FileName = e.ImageFileXfer.FileName });
            }
        }

        private void Twain32_XferDone(object sender, Twain32.XferDoneEventArgs e)
        {
            Console.Out.WriteLine("Twain32_XferDone");
            CheckStatus(e);
        }

        private void Twain32_AcquireCompleted(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Twain32_AcquireCompleted");
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

        private void Twain32_AcquireError(object sender, Twain32.AcquireErrorEventArgs e)
        {
            Console.Out.WriteLine("Twain32_AcquireError");
            if (ScanCompleted != null)
            {
                ScanCompleted(this, new ScanEvent());
            }
        }

    }
}
