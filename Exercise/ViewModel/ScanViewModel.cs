using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using TalBase.ViewModel;
using Panuon.UI;

namespace Exercise.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {

        public Page LastPage { get; private set; }
        public int ExcecisePageCount { get; private set; }
        public int PageCount { get; private set; }
        public int StudentCount { get; private set; }

        public RelayCommand ContinueCommand { get; set; }
        public RelayCommand EndScanCommand { get; set; }
        public RelayCommand DiscardCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public ScanViewModel()
        {
            ContinueCommand = new RelayCommand((e) => Continue(e));
            EndScanCommand = new RelayCommand((e) => EndScan(e));
            DiscardCommand = new RelayCommand((e) => Discard(e));
            scanModel.Pages.CollectionChanged += Pages_CollectionChanged;
            exerciseModel.PageStudents.CollectionChanged += PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
        }

        private async Task EndScan(object obj)
        {
            bool? isConfirm = PUMessageBox.ShowConfirm("扫描仪中还有试卷待扫描，确认结束扫描并查看结果吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                exerciseModel.MakeResult();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
        }

        private async Task Close(object obj)
        {
          
            bool? isConfirm = PUMessageBox.ShowConfirm("当前仍有扫描任务进行中，退出后本次扫描结果将放弃，确认退出吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }

        }

        private async Task Discard(object obj)
        {
            bool? isConfirm = PUMessageBox.ShowConfirm("放弃后，本次扫描结果将作废，确认放弃吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
        }

        private async Task Continue(object obj)
        {
            bool? isConfirm = PUMessageBox.ShowConfirm("扫描仪已无试卷，请添加试卷继续扫描。若已全部扫描，可查看扫描结果。", "提示");
            if (isConfirm != null && !isConfirm.Value)
            {
                exerciseModel.MakeResult();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
            else
            {
                await scanModel.Scan();
            }
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
            if (e.NewItems == null)
                return;
            Page page = e.NewItems[0] as Page;
            LastPage = page;
            RaisePropertyChanged("LastPage");
            PageCount = scanModel.Pages.Count;
            RaisePropertyChanged("PageCount");
        }
    }
}
