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
    public class ScanningViewModel : ScanViewModel
    {

        #region Properties

        public int _excecisePageCount;
        public int ExcecisePageCount
        {
            get { return _excecisePageCount; }
            private set{ _excecisePageCount = value ; RaisePropertyChanged("ExcecisePageCount"); }
        }
        public int _studentCount;
        public int StudentCount
        {
            get { return _studentCount; }
            private set { _studentCount = value; RaisePropertyChanged("StudentCount"); }
        }

        #endregion

        #region Commands

        public RelayCommand DiscardCommand { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public ScanningViewModel()
        {
            DiscardCommand = new RelayCommand((e) => Discard(e));
            exerciseModel.PageStudents.CollectionChanged += PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
        }

        #region Command Implements

        protected override async Task EndScan(object obj)
        {
            scanModel.PauseScan();
            bool? isConfirm = PUMessageBox.ShowConfirm("扫描仪中还有试卷待扫描，确认结束扫描并查看结果吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                await exerciseModel.MakeResult();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
            else
            {
                scanModel.ResumeScan();
            }
        }

        protected override async Task Close(object obj)
        {
            scanModel.PauseScan();
            bool? isConfirm = PUMessageBox.ShowConfirm("当前仍有扫描任务进行中，退出后本次扫描结果将放弃，确认退出吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
            else
            {
                scanModel.ResumeScan();
            }
        }

        private async Task Discard(object obj)
        {
            scanModel.PauseScan();
            bool? isConfirm = PUMessageBox.ShowConfirm("放弃后，本次扫描结果将作废，确认放弃吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
            else
            {
                scanModel.ResumeScan();
            }
        }

        private async Task Finish(object obj)
        {
            bool? isConfirm = PUMessageBox.ShowConfirm("扫描仪已无试卷，请添加试卷继续扫描。若已全部扫描，可查看扫描结果。", "提示");
            if (isConfirm != null && !isConfirm.Value)
            {
                await exerciseModel.MakeResult();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
            else
            {
                base.Continue(obj);
            }
        }

        #endregion

        private void PageStudents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StudentCount = exerciseModel.PageStudents.Count;
        }

        private void ExerciseModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExerciseData")
            {
                ExcecisePageCount = exerciseModel.ExerciseData.Pages.Count;
            }
        }

    }
}
