using Base.Mvvm;
using Exercise.Model;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        public Page LastPage { get; private set; }
        public int ExcecisePageCount { get; private set; }
        public int PageCount { get; private set; }
        public int StudentCount { get; private set; }

        public RelayCommand StartScanCommand { get; set; }
        public RelayCommand StopScanCommand { get; set; }
        public RelayCommand EndScanCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }


        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public ScanViewModel()
        {
            StartScanCommand = new RelayCommand((e) => scanModel.Scan());
            StopScanCommand = new RelayCommand((e) => scanModel.CancelScan());
            EndScanCommand = new RelayCommand((e) => exerciseModel.MakeResult());
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            exerciseModel.PageStudents.CollectionChanged += PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
        }

        private void PageStudents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StudentCount = exerciseModel.PageStudents.Count;
            RaisePropertyChanged("StudentCount");
        }

        private void ExerciseModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExerciseData")
            {
                ExcecisePageCount = exerciseModel.ExerciseData.Pages.Count;
                RaisePropertyChanged("ExcecisePageCount");
            }
        }

        private void Pages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Page page = e.NewItems[0] as Page;
            LastPage = page;
            RaisePropertyChanged("LastPage");
            PageCount = scanModel.Pages.Count;
            RaisePropertyChanged("PageCount");
        }
    }
}
