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

        #region Properties

        public ObservableCollection<Record> LocalRecords => historyModel.LocalRecords;

        private ICollection<Record> _Records;
        public ICollection<Record> Records
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
            get => _PageIndex;
            set
            {
                _PageIndex = value;
                SelectPage(value);
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
            }
        }

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
            new RelayCommand((o) => historyModel.Load()).Execute(null);
            historyModel.PropertyChanged += HistoryModel_PropertyChanged;
            if (historyModel.Records != null)
                PageCount = historyModel.Records.Length;
            PageIndex = 0;
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
            if (historyModel.Records != null && PageIndex < historyModel.Records.Length)
                Records = historyModel.Records[PageIndex];
            new RelayCommand((o) => historyModel.LoadMore(value)).Execute(null);
        }

        private void HistoryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Records")
            {
                if (historyModel.Records != null)
                {
                    if (PageIndex < historyModel.Records.Length)
                        Records = historyModel.Records[PageIndex];
                    PageCount = historyModel.Records.Length;
                }
            }
        }

        public async Task ModifyRecordName(Record record, string old)
        {
            var records = Records;
            await historyModel.ModifyRecord(record, old);
        }
    }
}
