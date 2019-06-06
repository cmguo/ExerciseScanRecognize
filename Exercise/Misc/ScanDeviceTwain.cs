using scan2web;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Exercise.Misc
{
    class ScanDeviceTwain : IScanDevice
    {

        private WpfTwain Twain;

        public ScanDeviceTwain()
        {
            Twain = new WpfTwain();
        }

        public void Select()
        {
            Twain.Select();
        }

        public bool Connect()
        {
            return true;
        }

        public async Task<List<ImageSource>> Scan()
        {
            TaskCompletionSource<List<ImageSource>> task = new TaskCompletionSource<List<ImageSource>>();
            Twain.Acquire(false);
            TwainTransferReadyHandler onReady = (s, images2) =>
            {
                task.TrySetResult(images2);
            };
            TwainEventHandler onClose = (s) =>
            {
                task.TrySetCanceled();
            };
            Twain.TwainTransferReady += onReady;
            Twain.TwainCloseRequest += onClose;
            List<ImageSource> images = await task.Task;
            Twain.TwainTransferReady -= onReady;
            Twain.TwainCloseRequest -= onClose;
            return images;
        }

        public void Close()
        {
            Twain = null;
        }
    }

}
