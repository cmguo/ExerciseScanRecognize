using Base.Misc;
using Exercise.Algorithm;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool _IsScannig;
        public bool IsScanning
        {
            get { return _IsScannig; }
            set
            {
                _IsScannig = value;
                RaisePropertyChanged("IsScanning");
                lock (mutex)
                {
                    Monitor.PulseAll(mutex);
                }
            }
        }
        public string PageCode { get; private set; }

        private IScanDevice scanDevice = ScanDevice.Instance;
        private Algorithm.Algorithm algorithm = new Algorithm.Algorithm();

        private object mutex = new object();

        private string savePath;
        private int scanBatch = 0;
        private int scanIndex = 0;
        private Page lastPage;
        private ExerciseData exerciseData;

        public ScanModel()
        {
            Pages = new ObservableCollection<Page>();
            scanDevice.OnImage += ScanDevice_OnImage;
            scanDevice.GetFileName += ScanDevice_GetFileName;
            scanDevice.ScanCompleted += ScanDevice_ScanCompleted; ;
        }

        public void SetSavePath(string path)
        {
            savePath = path;
        }

        public async Task Scan(short count = -1)
        {
            if (IsScanning)
                return;
            IsScanning = true;
            ++scanBatch;
            scanIndex = 0;
            try
            {
                await Task.Run(() => {
                    scanDevice.Open();
                    scanDevice.Duplex = true;
                    //scanDevice.ImageFormat = "Jfif";
               });
                // 必须主线程
               scanDevice.Scan(count);
            }
            catch (Exception e)
            {
                IsScanning = false;
                throw e;
            }
        }

        public Task CancelScan()
        {
            scanDevice.CancelScan();
            return Task.Run(() =>
            {
                lock (mutex)
                {
                    while (IsScanning)
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
            foreach (Page p in data.Pages)
                Pages.Add(p);
            PageCode = data.PageCode;
            scanBatch = data.ScanBatch;
            savePath = path;
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
            Page page = new Page() { ScanBatch = scanBatch, ScanIndex = ++scanIndex, PagePath = fileName };
            if (lastPage == null)
            {
                lastPage = page;
                return;
            }
            Page page1 = lastPage;
            lastPage = null;
            Page page2 = page;
            Page[] pages = new Page[] { page1, page2 };
            await Task.Factory.StartNew(() => ScanTwoPage(pages));
            Pages.Add(pages[0]);
            lastPage = null;
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
                        pages[0].Another = pages[0];
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
                    Algorithm.QRCodeData code = algorithm.GetCode(new Algorithm.PageRaw() { imgBytes = page.PageData });
                    int split = code.paperInfo.IndexOf('_');
                    page.PaperCode = code.paperInfo.Substring(0, split);
                    page.PageIndex = Int32.Parse(code.paperInfo.Substring(split + 1));
                    page.StudentCode = code.studentInfo;
                }
            }
            catch (Exception e)
            {
                page.Exception = e;
            }
        }

        private void ScanPage(Page page)
        {
            PageData pageData = exerciseData.Pages[page.PageIndex + (page.Another == null ? 1 : 0)];
            pageData.ImgBytes = page.PageData;
            try
            {
                AnswerData answerData = algorithm.GetAnswer(pageData);
                page.Answer = answerData;
                page.PageData = answerData.RedressedImgBytes;
                answerData.RedressedImgBytes = null;
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(answerData.RedressedImgBytes);
                page.Md5Name = BitConverter.ToString(output).Replace("-", "").ToLower() + ".jpg";
                File.Delete(page.PagePath);
                page.PagePath = savePath + "\\" + page.Md5Name;
                FileStream fs = new FileStream(page.PagePath, FileMode.Create, FileAccess.Write);
                using (fs) { new MemoryStream(page.PageData).CopyTo(fs); }
            }
            catch (Exception e)
            {
                page.Exception = e;
            }
            finally
            {
                page.PageData = null;
            }
        }

        private void ScanDevice_GetFileName(object sender, ScanEvent e)
        {
            e.FileName = savePath + "\\" + scanBatch + "_" + scanIndex + ".jpg";
        }

        private async void ScanDevice_OnImage(object sender, ScanEvent e)
        {
            await AddImage(e.FileName);
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
            IsScanning = false;
            RaisePropertyChanged("IsScanning");
        }

    }
}
