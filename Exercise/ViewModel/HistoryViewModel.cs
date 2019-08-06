using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TalBase.View;
using TalBase.ViewModel;
using static Exercise.Service.HistoryData;

namespace Exercise.ViewModel
{
    class HistoryViewModel : ViewModelBase
    {

        private const int PAGE_BAR_COUNT = 6;

        #region Properties

        public ObservableCollection<Record> LocalRecords => historyModel.LocalRecords;

        public IList<Record> Records => historyModel.Records;

        public int PageCount => historyModel.PageCount;

        private int _PageIndex;
        public int PageIndex
        {
            get => _PageIndex + 1;
            set
            {
                _PageIndex = value - 1;
                SelectPage(_PageIndex);
                RaisePropertyChanged("PageIndex");
            }
        }

        public ObservableCollection<object> Pages { get; private set; }

        #endregion

        #region Commands

        public RelayCommand SummaryCommand { get; private set; }

        public RelayCommand DiscardCommand { get; private set; }

        public RelayCommand ReturnCommand { get; private set; }

        #endregion

        private HistoryModel historyModel = HistoryModel.Instance;

        public HistoryViewModel()
        {
            SummaryCommand = new RelayCommand((o) => Summary(o));
            DiscardCommand = new RelayCommand((o) => DiscardRemove(o as Record));
            ReturnCommand = new RelayCommand((o) => Return(o));
            Pages = new ObservableCollection<object>();
            new RelayCommand((o) => historyModel.Load()).Execute(null);
            historyModel.PropertyChanged += HistoryModel_PropertyChanged;
            UpdatePageCount();
            PageIndex = 1;
        }

        private void DiscardRemove(Record record)
        {
            int result = PopupDialog.Show(Application.Current.MainWindow,"放弃扫描任务", "放弃后，本次扫描结果将作废，确认放弃么？", 0, "放弃本次扫描", "取消");
            if (result == 0)
            {
                historyModel.Remove(record);
            }
        }

        public void ShiftPages(bool left)
        {
            if (left)
            {
                if (PageIndex > 1)
                    --PageIndex;
            }
            else
            {
                if (PageIndex < PageCount)
                    ++PageIndex;
            }
        }

        public override void Release()
        {
            base.Release();
            historyModel.PropertyChanged -= HistoryModel_PropertyChanged;
        }

        private async Task Summary(object obj)
        {
            object[] args = obj as object[];
            try
            {
                await ExerciseModel.Instance.Load((args[1] as Record).LocalPath);
            }
            catch
            {
                ExerciseModel.Instance.Clear();
                throw;
            }
            (args[0] as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

        private void Return(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
        }

        private void SelectPage(int value)
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(AdjustPages);
            BackgroundWork.Execute(() => historyModel.LoadPage(value));
        }

        private void AdjustPages()
        {
            if (PageCount <= PAGE_BAR_COUNT)
            {
                if (Pages.Count > PageCount)
                    Pages.Clear();
                for (int i = Pages.Count; i < PageCount; ++i)
                    Pages.Add(i + 1);
            }
            else
            {
                Pages.Clear();
                int pageStart = PageIndex - PAGE_BAR_COUNT / 2;
                int pageEnd = pageStart + PAGE_BAR_COUNT - 1;
                if (pageStart < 1)
                {
                    pageEnd += 1 - pageStart;
                    pageStart = 1;
                }
                else if (pageEnd > PageCount)
                {
                    pageStart += PageCount - pageEnd;
                    pageEnd = PageCount;
                }
                Pages.Add(false);
                for (int i = pageStart; i <= pageEnd; ++i)
                    Pages.Add(i);
                Pages.Add(true);
            }
            RaisePropertyChanged("PageIndex");
        }

        private void HistoryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCount")
            {
                UpdatePageCount();
            }
            RaisePropertyChanged(e.PropertyName);
        }

        public async Task ModifyRecordName(Record record, string old)
        {
            await historyModel.ModifyRecord(record, old);
        }

        private void UpdatePageCount()
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(AdjustPages);
        }

    }
}
