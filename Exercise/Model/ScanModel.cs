﻿using Base.Misc;
using Base.Mvvm;
using Exercise.Algorithm;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TalBase.Model;

namespace Exercise.Model
{
    public class ScanModel : ModelBase
    {

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

        public string[] SourceList
        {
            get
            {
                scanDevice.Open();
                return scanDevice.SourceList;
            }
        }

        public int SourceIndex
        {
            get => scanDevice.SourceIndex;
            set => scanDevice.SourceIndex = value;
        }

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

        private IScanDevice scanDevice = ScanDevice.Instance;
        private Algorithm.Algorithm algorithm = new Algorithm.Algorithm();

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
            scanDevice.OnImage += ScanDevice_OnImage;
            scanDevice.GetFileName += ScanDevice_GetFileName;
            scanDevice.ScanPaused += ScanDevice_ScanPaused;
            scanDevice.ScanError += ScanDevice_ScanError;
            scanDevice.ScanCompleted += ScanDevice_ScanCompleted;
        }

        public void SetSavePath(string path)
        {
            savePath = path;
        }

        public void Scan(short count = -1)
        {
            if (IsScanning)
                return;
            Debug.WriteLine("Scan");
            IsScanning = true;
            IsPaused = false;
            IsCompleted = false;
            Error = null;
            cancel = false;
            ++scanBatch;
            scanIndex = 0;
            readIndex = 0;
            try
            {
                scanDevice.DuplexEnabled = true;
                scanDevice.Scan(count);
            }
            catch (Exception e)
            {
                IsScanning = false;
                throw e;
            }
        }

        public async Task<bool> PauseScan()
        {
            Debug.WriteLine("PauseScan");
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
            Debug.WriteLine("ResumeScan");
            IsPaused = false;
            scanDevice.ResumeScan();
        }

        public Task CancelScan()
        {
            Debug.WriteLine("CancelScan");
            cancel = true;
            scanDevice.CancelScan();
            return Task.Run(() =>
            {
                lock (mutex)
                {
                    while (IsScanning && !IsCompleted)
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
            int index = Pages.IndexOf(page);
            File.Delete(page.PagePath);
            page.PagePath = null;
            if (index >= 0)
                Pages[index] = page.Another;
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
            scanBatch = data.ScanBatch;
            savePath = path;
            foreach (Page p in data.Pages)
            {
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
            scanBatch = 0;
            scanIndex = 0;
            savePath = null;
            lastPage = null;
            PageCode = null;
            RaisePropertyChanged("PageCode");
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
            Debug.WriteLine("AddImage " + fileName);
            Page page2 = page;
            Page[] pages = new Page[] { page1, page2 };
            await Task.Factory.StartNew(() => ScanTwoPage(pages));
            Pages.Add(pages[0]);
            if (index + 1 == readIndex)
                IsCompleted = true;
        }

        private void ScanTwoPage(Page[] pages)
        {
            ReadPage(pages[0], true);
            if (pages[0].Exception != null)
            {
                ReadPage(pages[1], true);
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
                ReadPage(pages[1], false);
            }
            pages[1].PaperCode = pages[0].PaperCode;
            pages[1].PageIndex = pages[0].PageIndex + 1;
            pages[0].Another = pages[1];
            if (pages[0].Exception == null)
            {
                if (PageCode == null)
                {
                    PageCode = pages[0].PaperCode;
                    RaisePropertyChanged("PageCode");
                }
                if (PageCode == pages[0].PaperCode)
                {
                    lock (mutex)
                    {
                        while (exerciseData == null)
                        {
                            Monitor.Wait(mutex);
                        }
                    }
                    ScanPage(pages[0]);
                    if (pages[1].PageIndex < exerciseData.Pages.Count)
                    {
                        ScanPage(pages[1]);
                    }
                    else
                    {
                        pages[0].Another = null;
                        ReleasePage(pages[1]);
                    }
                }
            }
            pages[0].PageData = null;
            pages[1].PageData = null;
        }

        private void ReadPage(Page page, bool needCode)
        {
            try
            {
                FileStream fs = new FileStream(page.PagePath, FileMode.Open, FileAccess.Read);
                MemoryStream stream = new MemoryStream((int)fs.Length);
                using (fs) { fs.CopyTo(stream); }
                page.PageData = stream.GetBuffer();
                if (needCode)
                {
                    Algorithm.QRCodeData code = algorithm.GetCode(new Algorithm.PageRaw() { ImgBytes = page.PageData });
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
                page.Exception = e;
            }
        }

        private void ScanPage(Page page)
        {
            PageData pageData = exerciseData.Pages[page.PageIndex];
            pageData.ImgBytes = page.PageData;
            page.MetaData = pageData;
            try
            {
                AnswerData answerData = algorithm.GetAnswer(pageData);
                page.Answer = answerData;
                page.PageData = answerData.RedressedImgBytes;
                answerData.RedressedImgBytes = null;
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(page.PageData);
                page.PageName = BitConverter.ToString(output).Replace("-", "").ToLower() + ".jpg";
                File.Delete(page.PagePath);
                page.PagePath = savePath + "\\" + page.PageName;
                FileStream fs = new FileStream(page.PagePath, FileMode.Create, FileAccess.Write);
                using (fs) { new MemoryStream(page.PageData).CopyTo(fs); }
                page.PageData = null;
                fs.Close();
            }
            catch (Exception e)
            {
                page.Exception = e;
            }
            finally
            {
                page.PageData = null;
                pageData.ImgBytes = null;
            }
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
            Debug.WriteLine("ScanDevice_ScanPaused");
            IsPaused = true;
        }

        private void ScanDevice_ScanError(object sender, ScanEvent e)
        {
            Debug.WriteLine("ScanDevice_ScanError");
            Error = e.Error;
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
            Debug.WriteLine("ScanDevice_ScanCompleted");
            IsScanning = false;
            if (0 == readIndex)
                IsCompleted = true;
        }

    }
}
