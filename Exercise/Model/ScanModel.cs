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
        private Page _LastPage;
        public Page LastPage
        {
            get => _LastPage;
            private set
            {
                _LastPage = value;
                RaisePropertyChanged("LastPage");
            }
        }

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

        public bool IsDropped => cancel == CANCEL_DROP;

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

        private Exception _ScanException;
        public Exception ScanException
        {
            get { return _ScanException; }
            set
            {
                _ScanException = value;
                RaisePropertyChanged("ScanException");
            }
        }

        private IScanDevice scanDevice = new ScanDeviceSaraff(Application.Current.MainWindow);
        private Algorithm.Algorithm algorithm;

        private object mutex = new object();

        private string savePath;
        private int scanBatch = 0;
        private int scanIndex = 0;
        private int readIndex = 0;
        private int cancel = 0;
        private PagePair halfPage;
        private ExerciseData exerciseData;

        public ScanModel()
        {
            algorithm = new Algorithm.Algorithm(true, (m) =>
            {
                Base.Mvvm.Action.RaiseException(this, new AlgorithmException(0, m));
            });
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
            ScanException = null;
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
                ScanException = e;
                throw;
            }
        }

        public Task CancelScan(bool drop)
        {
            Log.d("CancelScan " + drop);
            lock (mutex)
            {
                cancel = (drop || cancel == CANCEL_DROP) ? CANCEL_DROP : CANCEL_STOP;
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
            Log.d("AddImage " + fileName);
            int n1 = fileName.LastIndexOf('_') + 1;
            int n2 = fileName.LastIndexOf(".");
            int index = Int32.Parse(fileName.Substring(n1, n2 - n1));
            Page page = new Page() { ScanBatch = scanBatch, ScanIndex = index, PagePath = fileName,
                PageName = fileName.Substring(fileName.LastIndexOf('\\') + 1) };
            PagePair pages = halfPage;
            if (pages == null)
            {
                halfPage = pages = new PagePair(page);
                page = null;
            }
            else
            {
                halfPage = null;
            }
            if (!await ScanTwoPage(pages, page))
                return;
            if (pages.Page1.Another != null)
            {
                LastPage = pages.Page2;
                pages.Page2.Another = null;
            }
            pages.Page1.TotalIndex = Pages.Count;
            pages.Page2.TotalIndex = Pages.Count;
            if (pages.Page1.Another == null)
                ReleasePage(pages.Page2);
            int drop = PageDropped.Count;
            try
            {
                Pages.Add(pages.Page1);
            }
            catch (Exception e)
            {
                Log.d(e);
            }
            pages.Page1 = null;
            pages.Page2 = null;
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

        private int[] elapseCounts = new int[4];
        private long[] elapseTicks = new long[4];

        private long AddTick(int index, long tick)
        {
            long tick1 = Environment.TickCount;
            Interlocked.Increment(ref elapseCounts[index]);
            Interlocked.Add(ref elapseTicks[index], tick1 - tick);
            return tick1;
        }

        private async Task<bool> ScanTwoPage(PagePair pages, Page page2)
        {
            long tick = Environment.TickCount;
            if (page2 == null)
            {
                tick = await ReadPage(pages.Page1, tick);
                // 识别正面二维码
                int task = pages.SetPageCode(true);
                if (task == 0) // 正面二维码没有识别出来，反面图片还没有到达
                    return false; // 直接返回，反面图片到达后，会继续处理
                if (task == 2) // 正面二维码没有识别出来，反面图片已经到达
                {
                    // 识别反面二维码
                    tick = await ReadPage(pages.Page2, tick);
                    if (pages.SetPageCode(false) == 0) // 反面二维码没有识别出来
                        return true; // 放弃，结束
                    task = 3;
                }
                // task == 1 // // 正面二维码已经识别出来，反面图片还没有到达
                // task == 3 // // 正面二维码已经识别出来，反面图片已经到达
                if (!await WaitExerciseData(pages.PaperCode))
                    return task == 3 || pages.Finish(true);
                tick = AddTick(1, tick);
                await ScanPage(pages.Page1, tick); // 分析正面
                bool finish = pages.Finish(true); // 可能反面分析也已经完成
                LastPage = pages.Page1;
                if (task == 3)
                {
                    if (pages.Page2.PageIndex < exerciseData.Pages.Count)
                        await ScanPage(pages.Page2, tick);
                    else
                        pages.DropPage2();
                    finish = pages.Finish(false);
                }
                return finish;
            }
            else
            {
                int task = pages.SetAnotherPage(page2);
                if (task == 0) // 正在分析正面二维码
                    return false; // 不等待，直接返回，正面分析完成后，会继续处理
                if (task == 3) // 正面分析二维码已经失败，需要尝试反面
                {
                    tick = await ReadPage(pages.Page2, tick);
                    if (pages.SetPageCode(false) == 0) // 反面二维码没有识别出来
                        return true; // 放弃，结束
                }
                // task == 2 or 3 // 二维码识别完成，继续分析
                if (!await WaitExerciseData(pages.PaperCode))
                    return task == 3 || pages.Finish(false); // 可能正面分析已经完成
                tick = AddTick(1, tick);
                bool finish = false;
                if (task == 3) // 可能需要分析正面
                {
                    tick = await ScanPage(pages.Page1, tick);
                    finish = pages.Finish(true);
                    LastPage = pages.Page1;
                }
                if (pages.Page2.PageIndex < exerciseData.Pages.Count)
                    tick = await ScanPage(pages.Page2, tick);
                else
                    pages.DropPage2();
                finish = pages.Finish(false);
                return finish;
            }
        }

        private async Task<bool> WaitExerciseData(string code)
        {
            if (PaperCode == null)
            {
                PaperCode = code;
                Log.d("PageCode=" + PaperCode);
                RaisePropertyChanged("PageCode");
            }
            if (PaperCode != code)
                return false;
            if (exerciseData == null)
            {
                await Task.Run(() =>
                {
                    lock (mutex)
                    {
                        while (exerciseData == null && cancel != CANCEL_DROP)
                        {
                            Monitor.Wait(mutex);
                        }
                    }
                });
            }
            if (exerciseData == null)
                return false;
            return true;
        }

        private async Task<long> ReadPage(Page page, long tick)
        {
            try
            {
                QRCodeData code = await algorithm.GetCode(new PageRaw() { ImgPathIn = page.PagePath });
                tick = AddTick(0, tick);
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
                tick = AddTick(2, tick);
                page.Answer = answerData;
                using (FileStream fs = new FileStream(outPath, FileMode.Open, FileAccess.Read))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] output = md5.ComputeHash(fs);
                    page.PageName = BitConverter.ToString(output).Replace("-", "").ToLower() + ".jpg";
                }
                tick = AddTick(3, tick);
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
            ScanException = e.Exception;
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
            Log.d("ScanDevice_ScanCompleted");
            IsScanning = false;
            if (halfPage != null)
            {
                try
                {
                    File.Delete(halfPage.Page1.PagePath);
                }
                catch
                {
                }
                halfPage = null;
                --readIndex;
            }
            if (Pages.Count * 2 == readIndex)
                IsCompleted = true;
        }

    }
}
