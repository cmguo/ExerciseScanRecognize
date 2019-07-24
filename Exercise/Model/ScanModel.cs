using Base.Misc;
using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Scanning;
using Exercise.Service;
using net.sf.jni4net;
using net.sf.jni4net.jni;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TalBase.Model;

namespace Exercise.Model
{
    public class ScanModel : ModelBase
    {

        private static readonly Logger Log = Logger.GetLogger<ScanModel>();

        private static ScanModel s_instance;
        public static ScanModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ScanModel();
                }
                return s_instance;
            }
        }

        public ObservableCollection<Page> Pages { get; private set; }
        public ObservableCollection<Page> PageDropped { get; private set; }

        public string[] SourceList
        {
            get
            {
                return scanDevice.SourceList;
            }
        }

        public int SourceIndex
        {
            get => scanDevice.SourceIndex;
            set => scanDevice.SourceIndex = value;
        }

        public bool FeederEnabled
        {
            get => scanDevice.FeederEnabled;
            set => scanDevice.FeederEnabled = value;
        }

        public bool PaperDectectable => scanDevice.PaperDectectable;

        public bool FeederLoaded => scanDevice.FeederLoaded;

        public string PageCode { get; private set; }

        private bool _IsScannig;
        public bool IsScanning
        {
            get { return _IsScannig; }
            set
            {
                if (_IsScannig == value)
                    return;
                lock (mutex)
                {
                    _IsScannig = value;
                    Monitor.PulseAll(mutex);
                }
                RaisePropertyChanged("IsScanning");
            }
        }

        private bool _IsPaused;
        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                if (_IsPaused == value)
                    return;
                lock (mutex)
                {
                    _IsPaused = value;
                    Monitor.PulseAll(mutex);
                }
                RaisePropertyChanged("IsPaused");
            }
        }

        private bool _IsCompleted;
        public bool IsCompleted
        {
            get { return _IsCompleted; }
            set
            {
                if (_IsCompleted == value)
                    return;
                lock (mutex)
                {
                    _IsCompleted = value;
                    Monitor.PulseAll(mutex);
                }
                if (!cancel)
                    RaisePropertyChanged("IsCompleted");
            }
        }

        private Exception _Error;
        public Exception Error
        {
            get { return _Error; }
            set
            {
                _Error = value;
                RaisePropertyChanged("Error");
            }
        }

        private IScanDevice scanDevice = new ScanDeviceSaraff(Application.Current.MainWindow);
        private Algorithm.Algorithm algorithm = new Algorithm.Algorithm(true);

        private object mutex = new object();

        private string savePath;
        private int scanBatch = 0;
        private int scanIndex = 0;
        private int readIndex = 0;
        private bool cancel = false;
        private Page lastPage;
        private ExerciseData exerciseData;

        public ScanModel()
        {
            Pages = new ObservableCollection<Page>();
            PageDropped = new ObservableCollection<Page>();
            scanDevice.OnImage += ScanDevice_OnImage;
            scanDevice.GetFileName += ScanDevice_GetFileName;
            scanDevice.ScanPaused += ScanDevice_ScanPaused;
            scanDevice.ScanError += ScanDevice_ScanError;
            scanDevice.ScanCompleted += ScanDevice_ScanCompleted;
            IsCompleted = true;
            scanDevice.Open();
            //BackgroudWork.Execute(DetectSource);
        }

        public override void Shutdown()
        {
            algorithm.Shutdown();
        }

        public void SetSavePath(string path)
        {
            savePath = path;
        }

        private async Task DetectSource()
        {
            await Task.Run(() => scanDevice.DetectSource());
            RaisePropertyChanged("SourceIndex");
        }

        public void CheckStatus()
        {
            scanDevice.CheckStatus();
        }

        public void Scan(short count = -1)
        {
            if (IsScanning)
                return;
            Log.d("Scan");
            IsScanning = true;
            IsPaused = false;
            IsCompleted = false;
            Error = null;
            cancel = false;
            ++scanBatch;
            scanIndex = 0;
            try
            {
                scanDevice.DuplexEnabled = true;
                scanDevice.Scan(count);
            }
            catch
            {
                IsScanning = false;
                throw;
            }
        }

        public async Task<bool> PauseScan()
        {
            Log.d("PauseScan");
            scanDevice.PauseScan();
            await Task.Run(() =>
            {
                lock (mutex)
                {
                    while (IsScanning && !IsPaused)
                        Monitor.Wait(mutex);
                }
            });
            return IsScanning;
        }

        public void ResumeScan()
        {
            Log.d("ResumeScan");
            IsPaused = false;
            scanDevice.ResumeScan();
        }

        public Task CancelScan()
        {
            Log.d("CancelScan");
            lock (mutex)
            {
                cancel = true;
                Monitor.PulseAll(mutex);
            }
            scanDevice.CancelScan();
            return Task.Run(() =>
            {
                lock (mutex)
                {
                    while (IsScanning || !IsCompleted)
                        Monitor.Wait(mutex);
                }
            });
        }

        public void SetExerciseData(ExerciseData data)
        {
            lock (mutex)
            {
                exerciseData = data;
                Monitor.PulseAll(mutex);
            }
        }

        public void ReleasePage(Page page)
        {
            File.Delete(page.PagePath);
            page.PagePath = null;
            if (page.TotalIndex < Pages.Count)
            {
                Page p = Pages[page.TotalIndex];
                if (p.Another == page)
                    p.Another = null;
                else if (p != page)
                    throw new InvalidOperationException(
                        "Page at " + page.TotalIndex + " not match " + p.TotalIndex);
                else if (IsCompleted)
                    Pages[page.TotalIndex] = p.Another;
            }
            PageDropped.Add(page);
        }

        public class PersistData
        {
            public ICollection<Page> Pages;
            public string PageCode;
            public int ScanBatch = 0;
        }

        public async Task Save()
        {
            PersistData data = new PersistData()
            {
                Pages = Pages,
                PageCode = PageCode,
                ScanBatch = scanBatch,
            };
            await JsonPersistent.Save(savePath + "\\scan.json", data);
        }

        public async Task Load(string path)
        {
            PersistData data = await JsonPersistent.Load<PersistData>(path + "\\scan.json");
            PageCode = data.PageCode;
            readIndex = data.Pages.Count * 2;
            scanBatch = data.ScanBatch;
            savePath = path;
            foreach (Page p in data.Pages)
            {
                if (p == null)
                {
                    Pages.Add(p);
                    continue;
                }
                if (p.PageName != null)
                {
                    p.PagePath = savePath + "\\" + p.PageName;
                }
                if (p.Another != null && p.Another.PageName != null)
                    p.Another.PagePath = savePath + "\\" + p.Another.PageName;
                if (p.PaperCode == PageCode)
                    p.MetaData = exerciseData.Pages[p.PageIndex];
                if (p.Another != null && p.Another.PaperCode == PageCode)
                    p.Another.MetaData = exerciseData.Pages[p.Another.PageIndex];
                Pages.Add(p);
            }
        }

        public void Clear()
        {
            readIndex = 0;
            scanBatch = 0;
            scanIndex = 0;
            savePath = null;
            lastPage = null;
            PageCode = null;
            RaisePropertyChanged("PageCode");
            PageDropped.Clear();
            Pages.Clear();
        }

        private async Task AddImage(string fileName)
        {
            int n1 = fileName.LastIndexOf('_') + 1;
            int n2 = fileName.LastIndexOf(".");
            int index = Int32.Parse(fileName.Substring(n1, n2 - n1));
            Page page = new Page() { ScanBatch = scanBatch, ScanIndex = index, PagePath = fileName,
                PageName = fileName.Substring(fileName.LastIndexOf('\\') + 1) };
            Page page1 = null;
            lock (mutex)
            {
                if (lastPage == null)
                {
                    lastPage = page;
                    return;
                }
                page1 = lastPage;
                lastPage = null;
            }
            Log.d("AddImage " + fileName);
            Page page2 = page;
            Page[] pages = new Page[] { page1, page2 };
            long tick = Environment.TickCount;
            //for (int i = 0; i < 1000; ++i)
            //{
                await ScanTwoPage(pages, tick);
                //await Task.Delay(500);
            //}
            pages[0].TotalIndex = Pages.Count;
            pages[1].TotalIndex = Pages.Count;
            if (pages[0].Another == null)
                ReleasePage(pages[1]);
            int drop = PageDropped.Count;
            try
            {
                Pages.Add(pages[0]);
            }
            catch (Exception e)
            {
                Log.d(e);
            }
            while (drop < PageDropped.Count)
            {
                Page p1 = PageDropped[drop++];
                Page p2 = Pages[p1.TotalIndex];
                if (p2 == null)
                    continue;
                else if (p2 == p1)
                    Pages[p1.TotalIndex] = p1.Another;
                else if (p2.Another == p1)
                    p2.Another = null;
                else if (p2.Another != null) // may already handled
                    throw new InvalidOperationException(
                        "Page at " + p1.TotalIndex + " not match " + p2.TotalIndex);
            }
            if (!IsScanning && Pages.Count * 2 == readIndex)
            {
                IsCompleted = true;
                for (int i = 0; i < elapseCounts.Length; ++i)
                {
                    Log.d("elapse [" + i + "]: count=" + elapseCounts[i] + ", ticks=" + elapseTicks[i]
                        + ", average=" + (elapseCounts[i] == 0 ? "NaN" : (elapseTicks[i] / elapseCounts[i]).ToString()));
                    elapseCounts[i] = 0;
                    elapseTicks[i] = 0;
                }
            }
        }

        private int[] elapseCounts = new int[7];
        private long[] elapseTicks = new long[7];

        private long AddTick(int index, long tick)
        {
            long tick1 = Environment.TickCount;
            Interlocked.Increment(ref elapseCounts[index]);
            Interlocked.Add(ref elapseTicks[index], tick1 - tick);
            return tick1;
        }

        private async Task ScanTwoPage(Page[] pages, long tick)
        {
            tick = AddTick(0, tick); // queue wait
            tick = await ReadPage(pages[0], true, tick); // tick 1,2
            if (pages[0].Exception != null)
            {
                tick = await ReadPage(pages[1], true, tick);
                if (pages[1].Exception == null)
                {
                    pages[0].Exception = null;
                    Page p = pages[0];
                    pages[0] = pages[1];
                    pages[1] = p;
                }
            }
            else
            {
                tick = await ReadPage(pages[1], false, tick); // tick 1,2
            }
            pages[1].PaperCode = pages[0].PaperCode;
            pages[1].PageIndex = pages[0].PageIndex + 1;
            pages[0].Another = pages[1];
            if (pages[0].Exception == null)
            {
                if (PageCode == null)
                {
                    Log.d("PageCode=" + PageCode);
                    PageCode = pages[0].PaperCode;
                    RaisePropertyChanged("PageCode");
                }
                if (PageCode == pages[0].PaperCode)
                {
                    if (exerciseData == null)
                    {
                        await Task.Run(() => WaitExerciseData());
                        if (exerciseData == null)
                            return;
                    }
                    tick = AddTick(3, tick);
                    tick = await ScanPage(pages[0], tick); // tick 4,5,6
                    if (pages[1].PageIndex < exerciseData.Pages.Count)
                    {
                        tick = await ScanPage(pages[1], tick);
                        tick = AddTick(6, tick);
                    }
                    else
                    {
                        pages[0].Another = null;
                    }
                }
            }
            long tick3 = Environment.TickCount;
        }

        private void WaitExerciseData()
        {
            lock (mutex)
            {
                while (exerciseData == null && !cancel)
                {
                    Monitor.Wait(mutex);
                }
            }
        }

        private async Task<long> ReadPage(Page page, bool needCode, long tick)
        {
            try
            {
                tick = AddTick(1, tick);
                if (needCode)
                {
                    Algorithm.QRCodeData code = await algorithm.GetCode(new Algorithm.PageRaw() { ImgPathIn = page.PagePath });
                    tick = AddTick(2, tick);
                    int split = code.PaperInfo.IndexOf('_');
                    if (split < 0)
                    {
                        page.PaperCode = code.PaperInfo;
                        page.PageIndex = 0;
                    }
                    else
                    {
                        page.PaperCode = code.PaperInfo.Substring(0, split);
                        page.PageIndex = Int32.Parse(code.PaperInfo.Substring(split + 1));
                    }
                    page.StudentCode = code.StudentInfo;
                }
            }
            catch (Exception e)
            {
                Log.w("Read " + e.Message + ": " + page.PagePath);
                page.Exception = e;
            }
            return tick;
        }

        private async Task<long> ScanPage(Page page, long tick)
        {
            PageData pageData = exerciseData.Pages[page.PageIndex];
            pageData.ImgPathIn = page.PagePath;
            pageData.ImgPathOut = page.PagePath.Replace(".jpg", ".out.jpg");
            page.MetaData = pageData;
            try
            {
                AnswerData answerData = await algorithm.GetAnswer(pageData);
                tick = AddTick(4, tick);
                page.Answer = answerData;
                //page.PageData = answerData.RedressedImgBytes;
                //answerData.RedressedImgBytes = null;
                using (FileStream fs = new FileStream(pageData.ImgPathOut, FileMode.Open, FileAccess.Read))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] output = md5.ComputeHash(fs);
                    page.PageName = BitConverter.ToString(output).Replace("-", "").ToLower() + ".jpg";
                }
                tick = AddTick(5, tick);
                File.Delete(page.PagePath);
                page.PagePath = savePath + "\\" + page.PageName;
                File.Move(pageData.ImgPathOut, page.PagePath);
                tick = AddTick(6, tick);
                //page.PageData = null;
            }
            catch (Exception e)
            {
                Log.w("Scan " + e.Message + ": " + page.PagePath);
                page.Exception = e;
            }
            finally
            {
                //page.PageData = null;
                //pageData.ImgBytes = null;
            }
            return tick;
        }

        private void ScanDevice_GetFileName(object sender, ScanEvent e)
        {
            e.FileName = savePath + "\\" + scanBatch + "_" + (scanIndex++) + ".jpg";
        }

        private void ScanDevice_OnImage(object sender, ScanEvent e)
        {
            ++readIndex;
            BackgroudWork.Execute(() => AddImage(e.FileName));
        }

        private void ScanDevice_ScanPaused(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanPaused");
            IsPaused = true;
        }

        private void ScanDevice_ScanError(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanError");
            Error = e.Error;
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanCompleted");
            IsScanning = false;
            if (lastPage != null)
            {
                File.Delete(lastPage.PagePath);
                lastPage = null;
                --readIndex;
            }
            if (Pages.Count * 2 == readIndex)
                IsCompleted = true;
        }

    }
}
