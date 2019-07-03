using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TalBase.ViewModel;
using static Exercise.Service.HistoryData;

namespace Exercise.ViewModel
{
    class HistoryViewModel : ViewModelBase
    {

        public ObservableCollection<Record> LocalRecords => historyModel.LocalRecords;
        public ObservableCollection<Record> Records => historyModel.Records;


        public RelayCommand SummaryCommand { get; private set; }

        public RelayCommand DiscardCommand { get; private set; }

        public RelayCommand LoadMoreCommand { get; private set; }

        private HistoryModel historyModel = HistoryModel.Instance;

        public HistoryViewModel()
        {
            SummaryCommand = new RelayCommand((o) => Summary(o));
            DiscardCommand = new RelayCommand((o) => historyModel.Remove(o as Record));
            LoadMoreCommand = new RelayCommand((o) => historyModel.LoadMore());
            new RelayCommand((o) => historyModel.Load()).Execute(null);
        }

        private async Task Summary(object o)
        {
            object[] args = o as object[];
            await ExerciseModel.Instance.Load((args[1] as Record).LocalPath);
            (args[0] as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }
    }
}
