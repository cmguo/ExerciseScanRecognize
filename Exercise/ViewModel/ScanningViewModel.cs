using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using TalBase.ViewModel;
using Panuon.UI;
using TalBase.View;

namespace Exercise.ViewModel
{
    public class ScanningViewModel : ExerciseViewModel
    {

        #region Properties

        public int _ExercisePageCount;
        public int ExercisePageCount
        {
            get { return _ExercisePageCount; }
            private set{ _ExercisePageCount = value ; RaisePropertyChanged("ExercisePageCount"); }
        }
        public int _studentCount;
        public int StudentSumary
        {
            get { return _studentCount; }
            private set { _studentCount = value; RaisePropertyChanged("StudentSumary"); }
        }

        #endregion

        #region Commands

        public RelayCommand FinishCommand { get; set; }
        public RelayCommand ErrorCommand { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public ScanningViewModel()
        {
            FinishCommand = new RelayCommand((e) => Finish(e));
            ErrorCommand = new RelayCommand((e) => OnError(e));
            exerciseModel.PageStudents.CollectionChanged += PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
            CloseMessage = "当前仍有扫描任务进行中，" + CloseMessage;
            StudentSumary = exerciseModel.PageStudents.Count;
        }

        public override void Release()
        {
            base.Release();
            exerciseModel.PageStudents.CollectionChanged -= PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged -= ExerciseModel_PropertyChanged;
        }

        #region Command Implements

        protected override async Task EndScan(object obj)
        {
            if (!await scanModel.PauseScan())
            {
                await exerciseModel.MakeResult();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
                return;
            }
            System.Windows.Controls.Page page = obj as System.Windows.Controls.Page;
            FrameworkElement element = page.Resources["ClassDetail"] as FrameworkElement;
            element.DataContext = this;
            int result = PopupDialog.Show("扫描仪中还有试卷待扫描，确认结束扫描并查看结果吗？", element, 0, "查看结果", "继续扫描");
            if (result == 0)
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

        private async Task OnError(object obj)
        {
            bool isScanning = IsScanning;
            if (isScanning)
                await scanModel.PauseScan();
            string msg = null;
            if (Error == 0)
                msg = "当前试卷二维码无法识别，不能查看结果";
            else if (Error == 1)
                msg = "当前试卷二维码无法识别，请检查试卷后重试";
            else if (Error == 2)
                msg = "数据连接异常，请联系服务人员";
            int result = PopupDialog.Show(msg, 0, "确定");
            if (result == 1)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
            else
            {
                if (isScanning)
                    scanModel.ResumeScan();
                else
                    base.Continue(obj);
            }
        }

        private async Task Finish(object obj)
        {
            Update();
            System.Windows.Controls.Page page = obj as System.Windows.Controls.Page;
            FrameworkElement element = page.Resources["ClassDetail"] as FrameworkElement;
            element.DataContext = this;
            int result = PopupDialog.Show("扫描仪已无试卷，请添加试卷继续扫描。若已全部扫描，可查看扫描结果。", element, 0, "查看结果", "继续扫描");
            if (result == 0)
            {
                if (scanModel.PageCode == null || exerciseModel.ExerciseData == null)
                {
                    await OnError(obj);
                    return;
                }
                await exerciseModel.MakeResult();
                page.NavigationService.Navigate(new SummaryPage());
            }
            else
            {
                base.Continue(obj);
            }
        }

        #endregion

        private void PageStudents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StudentSumary = exerciseModel.PageStudents.Count;
        }

        private void ExerciseModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExerciseData")
            {
                if (exerciseModel.ExerciseData != null)
                    ExercisePageCount = exerciseModel.ExerciseData.Pages.Count;
            }
        }

    }
}
