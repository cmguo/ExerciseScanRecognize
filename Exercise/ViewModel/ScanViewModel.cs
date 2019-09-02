using Base.Mvvm;
using Exercise.Model;
using System;
using System.Threading.Tasks;
using TalBase.ViewModel;
using TalBase.View;
using System.Windows;
using Base.Misc;
using System.Collections.Specialized;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        private static readonly Logger Log = Logger.GetLogger<ScanViewModel>();

        #region Properties

        public int SourceIndex
        {
            get => scanModel.SourceIndex;
            set { scanModel.SourceIndex = value; }
        }

        public int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            private set { _pageCount = value; RaisePropertyChanged("PageCount"); }
        }

        public int _Status;
        public int Status
        {
            get { return _Status; }
            protected set { _Status = value; RaisePropertyChanged("Status"); }
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
            ContinueCommand = new RelayCommand((e) => Continue(e), CanContinue);
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
            await scanModel.CancelScan(true);
        }

        protected bool Check(object obj)
        {
            try
            {
                scanModel.CheckStatus();
            }
            catch (Exception e)
            {
                Log.w("Check", e);
                ConfirmationRequest.RaiseForResult(new Confirmation
                {
                    Title = "发现错误",
                    Content = "扫描仪未正确连接，请检查后重试。",
                    DefaultButton = 0,
                    Buttons = new string[] { "确定" }, 
                    Owner = obj as UIElement
                });
                return false;
            }
            int result = 0;
            //while (result == 0 && !scanModel.FeederLoaded)
            //{
            //    result = 1; PopupDialog.Show(obj as UIElement, "发现错误", "扫描仪里面没有纸张，请添加试卷。", 0, "确定", "取消");
            //}
            return result == 0;
        }

        protected virtual bool CanContinue(object obj)
        {
            return true;
        }

        protected virtual bool Continue(object obj)
        {
            if (!Check(obj))
                return false;
            try
            {
                scanModel.Scan();
                return true;
            }
            catch (Exception e)
            {
                Log.w("Continue", e);
                return false;
            }
        }

        #endregion

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null)
                return;
            PageCount = scanModel.Pages.Count;
            if (PageCount >= 5 && scanModel.PaperCode == null)
                Status = 1;
        }

        private void ScanModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning"
                || e.PropertyName == "IsCompleted"
                || e.PropertyName == "SourceIndex")
                RaisePropertyChanged(e.PropertyName);
        }

    }
}
