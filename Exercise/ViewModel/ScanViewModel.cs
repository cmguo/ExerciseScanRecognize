using Base.Mvvm;
using Exercise.Model;
using System;
using System.Threading.Tasks;
using TalBase.ViewModel;
using Panuon.UI;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        #region Properties

        public Page _lastPage;
        public Page LastPage
        {
            get { return _lastPage; }
            private set { _lastPage = value; RaisePropertyChanged("LastPage"); }
        }

        public int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            private set { _pageCount = value; RaisePropertyChanged("PageCount"); }
        }

        #endregion

        #region Commands

        public RelayCommand ContinueCommand { get; set; }
        public RelayCommand EndScanCommand { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;

        public ScanViewModel()
        {
            ContinueCommand = new RelayCommand((e) => Continue(e));
            EndScanCommand = new RelayCommand((e) => EndScan(e));
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
        }

        #region Command Implements

        protected virtual async Task EndScan(object obj)
        {
            await Task.FromException(new NotImplementedException("EndScan"));
        }

        protected virtual async Task Close(object obj)
        {
            await Task.FromException(new NotImplementedException("Close"));
        }

        protected virtual void Continue(object obj)
        {
            while (!scanModel.FeederLoaded)
            {
                PUMessageBox.ShowConfirm("扫描仪里面没有纸张，请添加试卷。", "提示");
            }
            scanModel.Scan();
        }

        #endregion

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            Page page = e.NewItems[0] as Page;
            LastPage = page;
            PageCount = scanModel.Pages.Count;
        }
    }
}
