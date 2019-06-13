using Exercise.Algorithm;
using Exercise.Service;
using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public bool IsScanning { get; set; }
        public string PageCode { get; set; }

        private IScanDevice scanDevice = ScanDevice.Instance;
        private Algorithm.Algorithm algorithm = new Algorithm.Algorithm();

        private object mutex = new object();

        private string scanPath;
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

        public async void Scan(short count = -1)
        {
            if (IsScanning)
                return;
            IsScanning = true;
            RaisePropertyChanged("IsScanning");
            scanPath = System.Environment.CurrentDirectory 
                + "\\扫描试卷\\" + DateTime.Now.ToString("D") + "\\" + DateTime.Now.ToString("T").Replace(':', '.');
            Directory.CreateDirectory(scanPath);
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
                RaisePropertyChanged("IsScanning");
                await AddImage("");
                await AddImage("");
            }
        }

        public void CancelScan()
        {
            scanDevice.CancelScan();
        }

        public void SetExerciseData(ExerciseData data)
        {
            lock (mutex)
            {
                exerciseData = data;
                Monitor.PulseAll(mutex);
            }
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
            pages[1].PageCode = pages[0].PageCode;
            pages[1].PageIndex = pages[0].PageIndex + 1;
            pages[0].Another = pages[1];
            if (pages[0].Exception == null)
            {
                if (PageCode == null)
                {
                    PageCode = pages[0].PageCode;
                    RaisePropertyChanged("PageCode");
                }
                if (PageCode == pages[0].PageCode)
                {
                    lock (mutex)
                    {
                        while (exerciseData == null)
                        {
                            Monitor.Wait(mutex);
                        }
                    }
                    ScanPage(pages[0]);
                    if (pages[1].PageIndex < exerciseData.pages.Count)
                    {
                        ScanPage(pages[1]);
                    }
                    else
                    {
                        pages[0].Another = pages[0];
                        RemovePage(pages[1]);
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
                FileStream file = new FileStream(page.PagePath, FileMode.Open, FileAccess.Read);
                MemoryStream stream = new MemoryStream((int)file.Length);
                file.CopyTo(stream);
                page.PageData = stream.GetBuffer();
                if (needCode)
                {
                    Algorithm.QRCodeData code = algorithm.GetCode(new Algorithm.PageRaw() { imgBytes = page.PageData });
                    int split = code.paperInfo.IndexOf('_');
                    page.PageCode = code.paperInfo.Substring(0, split);
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
            PageData pageData = exerciseData.pages[page.PageIndex + (page.Another == null ? 1 : 0)];
            pageData.imgBytes = page.PageData;
            try
            {
                AnswerData answerData = algorithm.GetAnswer(pageData);
                page.Answer = answerData;
            }
            catch (Exception e)
            {
                page.Exception = e;
            }
        }

        private void RemovePage(Page page)
        {
            File.Delete(page.PagePath);
        }

        private void ScanDevice_GetFileName(object sender, ScanEvent e)
        {
            e.FileName = scanPath + "\\" + scanIndex + ".jpg";
        }

        private async void ScanDevice_OnImage(object sender, ScanEvent e)
        {
            await AddImage(e.FileName);
        }

        private void ScanDevice_ScanCompleted(object sender, ScanEvent e)
        {
        }

    }
}
