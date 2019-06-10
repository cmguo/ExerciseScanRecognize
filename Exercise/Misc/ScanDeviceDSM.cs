using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TWAINComm;

namespace Exercise.Misc
{
    class ScanDeviceDSM : IScanDevice
    {

        private TWAINComm.Twain Twain;
        private TaskCompletionSource<List<ImageSource>> Task;

        public ScanDeviceDSM()
        {
            TWAINComm.Feedback feedback = new TWAINComm.Feedback();
            feedback.ScanEnd += Twain_ScanEnd;
            feedback.ApplicationIdentityChanged += Twain_ApplicationIdentityChanged;
            feedback.DataSourceIdentityChanged += Twain_DataSourceIdentityChanged;
            feedback.TwainStateChanged += Twain_TwainStateChanged;
            feedback.TwainActionChanged += Twain_TwainActionChanged;
            feedback.TwainCommException += Twain_TwainCommException;
            Twain = new TWAINComm.Twain(App.Current.MainWindow, feedback);
        }

        public void Select()
        {
            Twain.SelectSource();
        }

        public bool Connect()
        {
            return true;
        }

        public async Task<List<ImageSource>> Scan()
        {
            Twain.ScanBegin();
            Task = new TaskCompletionSource<List<ImageSource>>();
            List<ImageSource> images = await Task.Task;
            Task = null;
            return images;
        }

        public void Close()
        {
            if (Twain != null)
            {
                Twain.Dispose();
                Twain = null;
            }
        }

        private void Twain_ScanEnd(List<string> pngImageFiles)
        {
            if (Task == null)
                return;
            if (pngImageFiles == null || pngImageFiles.Count == 0)
            {
                Task.TrySetException(new OperationCanceledException());
                return;
            }
            List<ImageSource> images = new List<ImageSource>();
            foreach (string file in pngImageFiles)
            {
                images.Add(new BitmapImage(new Uri(file)));
            }
            Task.TrySetResult(images);
        }

        private void Twain_ApplicationIdentityChanged(TW_IDENTITY twIdentity)
        {
        }

        private void Twain_DataSourceIdentityChanged(TW_IDENTITY twIdentity)
        {
        }

        private void Twain_TwainStateChanged(State twState)
        {
        }

        private void Twain_TwainActionChanged(string action)
        {
        }

        private void Twain_TwainCommException(Exception ex)
        {
            Console.Out.WriteLine(ex.ToString());
        }

    }

}
