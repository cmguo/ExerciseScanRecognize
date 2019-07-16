using Base.Mvvm;
using Exercise.Model;
using Exercise.Service;
using Exercise.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using TalBase.ViewModel;
using static Exercise.Service.HistoryData;

namespace Exercise.ViewModel
{
    class HistoryViewModel : ViewModelBase
    {

        private const int PAGE_BAR_COUNT = 6;

        #region Properties

        public ObservableCollection<Record> LocalRecords => historyModel.LocalRecords;

        private IList<Record> _Records;
        public IList<Record> Records
        {
            get => _Records;
            set
            {
                _Records = value;
                RaisePropertyChanged("Records");
            }
        }


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

        private int _PageCount;
        public int PageCount
        {
            get => _PageCount;
            set
            {
                if (_PageCount == value)
                    return;
                _PageCount = value;
                RaisePropertyChanged("PageCount");
                UpdatePageCount();
            }
        }

        public ObservableCollection<object> Pages { get; private set; }

        private int pageStart = 1;
        private int pageEnd = 5;

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
            DiscardCommand = new RelayCommand((o) => historyModel.Remove(o as Record));
            ReturnCommand = new RelayCommand((o) => Return(o));
            Pages = new ObservableCollection<object>();
            new RelayCommand((o) => historyModel.Load()).Execute(null);
            historyModel.PropertyChanged += HistoryModel_PropertyChanged;
            if (historyModel.Records != null && historyModel.Records.Length > 0)
            {
                PageCount = historyModel.Records.Length;
            }
            PageIndex = 1;
        }

        public void ShiftPages(bool left)
        {
            if (left)
            {
                if (pageStart > 1)
                {
                    --pageStart;
                    --pageEnd;
                    Pages.RemoveAt(5);
                    Pages.Insert(1, pageStart);
                }
            }
            else
            {
                if (pageEnd < PageCount - 1)
                {
                    ++pageStart;
                    ++pageEnd;
                    Pages.RemoveAt(1);
                    Pages.Insert(5, pageEnd);
                }
            }
            RaisePropertyChanged("PageIndex");
        }

        public override void Release()
        {
            base.Release();
            historyModel.PropertyChanged -= HistoryModel_PropertyChanged;
        }

        private async Task Summary(object obj)
        {
            object[] args = obj as object[];
            await ExerciseModel.Instance.Load((args[1] as Record).LocalPath);
            (args[0] as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

        private void Return(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
        }

        private void SelectPage(int value)
        {
            if (historyModel.Records != null && _PageIndex < historyModel.Records.Length)
                Records = historyModel.Records[_PageIndex];
            new RelayCommand((o) => historyModel.LoadMore(value)).Execute(null);
        }

        private void HistoryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Records")
            {
                if (historyModel.Records != null)
                {
                    if (_PageIndex < historyModel.Records.Length)
                        Records = historyModel.Records[_PageIndex];
                    PageCount = 10;// historyModel.Records.Length;
                }
            }
        }

        public async Task ModifyRecordName(Record record, string old)
        {
            await historyModel.ModifyRecord(record, old);
        }

        private void UpdatePageCount()
        {
            Pages.Clear();
            if (PageCount <= PAGE_BAR_COUNT)
            {
                for (int i = 1; i <= PageCount; ++i)
                    Pages.Add(i);
                pageEnd = PageCount;
            }
            else
            {
                Pages.Add(false);
                for (int i = 1; i <= 5; ++i)
                    Pages.Add(i);
                Pages.Add(true);
                pageEnd = 5;
            }
        }

    }
}
