using Base.Mvvm;
using Exercise.Model;
using System;
using System.Threading.Tasks;
using TalBase.ViewModel;
using Panuon.UI;
using TalBase.View;

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

        public int _error;
        public int Error
        {
            get { return _error; }
            protected set { _error = value; RaisePropertyChanged("Error"); }
        }

        public bool IsScanning => scanModel.IsScanning;
        public bool IsCompleted => scanModel.IsCompleted;

        #endregion

        #region Commands

        public RelayCommand ContinueCommand { get; set; }
        public RelayCommand EndScanCommand { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;

        public ScanViewModel()
        {
            PageCount = scanModel.Pages.Count;
            ContinueCommand = new RelayCommand((e) => Continue(e));
            EndScanCommand = new RelayCommand((e) => EndScan(e));
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            scanModel.PropertyChanged += ScanModel_PropertyChanged;
        }

        public override void Release()
        {
            base.Release();
            scanModel.Pages.CollectionChanged -= Pages_CollectionChanged;
            scanModel.PropertyChanged -= ScanModel_PropertyChanged;
        }

        #region Command Implements

        protected virtual async Task EndScan(object obj)
        {
            await scanModel.CancelScan();
        }

        protected virtual void Continue(object obj)
        {
            while (!scanModel.FeederLoaded)
            {
                PopupDialog.Show("扫描仪里面没有纸张，请添加试卷。", 0, "确定");
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
            if (PageCount >= 5 && scanModel.PageCode == null)
                Error = 1;
        }

        private void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning"
                || e.PropertyName == "IsCompleted"
                || e.PropertyName == "IsPaused")
                RaisePropertyChanged(e.PropertyName);
        }

    }
}
