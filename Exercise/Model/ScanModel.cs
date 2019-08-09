using Base.Misc;
using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Scanner;
using Exercise.Service;
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

        private const int CANCEL_DROP = 1;
        private const int CANCEL_STOP = 2;

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
        public Page LastPage { get; private set; }

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

        public string PaperCode { get; private set; }

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
                if (value)
                    HistoryModel.Instance.EndDuration(Pages.Count);
                if (cancel == 0)
                    RaisePropertyChanged("IsCompleted");
            }
        }

        private Exception _Exception;
        public Exception Exception
        {
            get { return _Exception; }
            set
            {
                _Exception = value;
                RaisePropertyChanged("Exception");
            }
        }

        private IScanDevice scanDevice = new ScanDeviceSaraff(Application.Current.MainWindow);
        private Algorithm.Algorithm algorithm = new Algorithm.Algorithm(true);

        private object mutex = new object();

        private string savePath;
        private int scanBatch = 0;
        private int scanIndex = 0;
        private int readIndex = 0;
        private int cancel = 0;
        private Page halfPage;
        private ExerciseData exerciseData;

        public ScanModel()
        {
            Pages = new ObservableCollection<Page>();
            PageDropped = new ObservableCollection<Page>();
            scanDevice.OnImage += ScanDevice_OnImage;
            scanDevice.GetFileName += ScanDevice_GetFileName;
            scanDevice.ScanException += ScanDevice_ScanException;
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
            IsCompleted = false;
            Exception = null;
            cancel = 0;
            ++scanBatch;
            scanIndex = 0;
            try
            {
                scanDevice.DuplexEnabled = true;
                scanDevice.Scan(count);
                if (count < 0)
                    HistoryModel.Instance.BeginDuration(HistoryModel.DurationType.Scan);
            }
            catch (Exception e)
            {
                cancel = CANCEL_DROP;
                IsScanning = false;
                IsCompleted = true;
                Exception = e;
                throw;
            }
        }

        public Task CancelScan(bool drop)
        {
            Log.d("CancelScan " + drop);
            lock (mutex)
            {
                cancel = drop ? CANCEL_DROP : CANCEL_STOP;
                Monitor.PulseAll(mutex);
            }
            scanDevice.CancelScan(drop);
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
                PageCode = PaperCode,
                ScanBatch = scanBatch,
            };
            await JsonPersistent.SaveAsync(savePath + "\\scan.json", data);
        }

        public async Task Load(string path)
        {
            PersistData data = await JsonPersistent.LoadAsync<PersistData>(path + "\\scan.json");
            PaperCode = data.PageCode;
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
                if (p.PaperCode == PaperCode)
                    p.MetaData = exerciseData.Pages[p.PageIndex];
                if (p.Another != null && p.Another.PaperCode == PaperCode)
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
            halfPage = null;
            LastPage = null;
            PaperCode = null;
            exerciseData = null;
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
                if (halfPage == null)
                {
                    halfPage = page;
                    return;
                }
                page1 = halfPage;
                halfPage = null;
            }
            Log.d("AddImage " + fileName);
            if (cancel == CANCEL_DROP)
            {
                page1.Another = page;
                Pages.Add(page1);
                return;
            }
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

        private int[] elapseCounts = new int[5];
        private long[] elapseTicks = new long[5];

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
            tick = await ReadPage(pages[0], tick); // tick 1
            if (pages[0].PaperCode == null)
            {
                tick = await ReadPage(pages[1], tick);
                if (pages[1].PaperCode != null)
                {
                    pages[0].Exception = null;
                    Page p = pages[0];
                    pages[0] = pages[1];
                    pages[1] = p;
                }
            }
            pages[1].PaperCode = pages[0].PaperCode;
            pages[1].PageIndex = pages[0].PageIndex + 1;
            pages[1].StudentCode = pages[0].StudentCode;
            pages[0].Another = pages[1];
            if (pages[0].PaperCode == null)
                return;
            if (PaperCode == null)
            {
                PaperCode = pages[0].PaperCode;
                Log.d("PageCode=" + PaperCode);
                RaisePropertyChanged("PageCode");
            }
            if (PaperCode != pages[0].PaperCode)
                return;
            if (exerciseData == null)
            {
                await Task.Run(() => WaitExerciseData());
                if (exerciseData == null)
                    return;
            }
            tick = AddTick(2, tick);
            tick = await ScanPage(pages[0], tick); // tick 3,4
            LastPage = pages[0];
            RaisePropertyChanged("LastPage");
            if (pages[1].PageIndex < exerciseData.Pages.Count)
            {
                tick = await ScanPage(pages[1], tick);
                LastPage = pages[1];
                RaisePropertyChanged("LastPage");
            }
            else
            {
                pages[0].Another = null;
            }
        }

        private void WaitExerciseData()
        {
            lock (mutex)
            {
                while (exerciseData == null && cancel != CANCEL_DROP)
                {
                    Monitor.Wait(mutex);
                }
            }
        }

        private async Task<long> ReadPage(Page page, long tick)
        {
            try
            {
                Algorithm.QRCodeData code = await algorithm.GetCode(new Algorithm.PageRaw() { ImgPathIn = page.PagePath });
                tick = AddTick(1, tick);
                if (code.PaperInfo != null)
                {
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
                    if (code.StudentInfo == null)
                    {
                        page.Exception = new NullReferenceException("学生二维码未识别");
                    }
                }
                else
                {
                    page.Exception = new NullReferenceException("试卷二维码未识别");
                }
                page.StudentCode = code.StudentInfo;
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
            string outPath = page.PagePath.Replace(".jpg", ".out.jpg");
            page.MetaData = pageData;
            try
            {
                AnswerData answerData = await algorithm.GetAnswer(pageData, page.PagePath, outPath);
                tick = AddTick(3, tick);
                page.Answer = answerData;
                using (FileStream fs = new FileStream(outPath, FileMode.Open, FileAccess.Read))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] output = md5.ComputeHash(fs);
                    page.PageName = BitConverter.ToString(output).Replace("-", "").ToLower() + ".jpg";
                }
                tick = AddTick(4, tick);
                File.Delete(page.PagePath);
                page.PagePath = savePath + "\\" + page.PageName;
                File.Move(outPath, page.PagePath);
            }
            catch (Exception e)
            {
                Log.w("Scan " + e.Message + ": " + page.PagePath);
                page.Exception = e;
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
            BackgroundWork.Execute(() => AddImage(e.FileName));
        }

        private void ScanDevice_ScanException(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanException");
            Exception = e.Exception;
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanCompleted");
            IsScanning = false;
            if (halfPage != null)
            {
                File.Delete(halfPage.PagePath);
                halfPage = null;
                --readIndex;
            }
            if (Pages.Count * 2 == readIndex)
                IsCompleted = true;
        }

    }
}
