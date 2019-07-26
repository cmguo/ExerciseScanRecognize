using Base.Misc;
using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using net.sf.jni4net.jni;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TalBase.View;

namespace Exercise.ViewModel
{
    public class ScanningViewModel : ExerciseViewModel
    {

        private static readonly Logger Log = Logger.GetLogger<ScanningViewModel>();

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

        public ObservableCollection<Tuple<string, long>> Usages { get; set; }

        #endregion

        #region Commands

        public RelayCommand FinishCommand { get; set; }
        public RelayCommand ErrorCommand { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private DispatcherTimer timer = new DispatcherTimer();

        public ScanningViewModel()
        {
            FinishCommand = new RelayCommand((e) => Finish(e));
            ErrorCommand = new RelayCommand((e) => OnError(e));
            exerciseModel.PageStudents.CollectionChanged += PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
            CloseMessage = "当前仍有扫描任务进行中，" + CloseMessage;
            StudentSumary = exerciseModel.PageStudents.Count;
            if (exerciseModel.ExerciseData != null)
                ExercisePageCount = exerciseModel.ExerciseData.Pages.Count;
            Usages = new ObservableCollection<Tuple<string, long>>();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public override void Release()
        {
            base.Release();
            timer.Stop();
            exerciseModel.PageStudents.CollectionChanged -= PageStudents_CollectionChanged;
            exerciseModel.PropertyChanged -= ExerciseModel_PropertyChanged;
        }

        #region Command Implements

        protected override async Task EndScan(object obj)
        {
            await scanModel.CancelScan(false);
            Update();
            System.Windows.Controls.Page page = obj as System.Windows.Controls.Page;
            FrameworkElement element = page.Resources["ClassDetail"] as FrameworkElement;
            element.DataContext = this;
            int result = PopupDialog.Show(obj as UIElement, "确认", "扫描仪中还有试卷待扫描，确认结束扫描并查看结果吗？", element, 0, "查看结果", "继续扫描");
            if (result == 0 && exerciseModel.ExerciseData == null)
            {
                result = PopupDialog.Show(obj as UIElement, "确认", "没有有效试卷信息", element, 0, "放弃扫描", "继续扫描");
            }
            if (result == 0)
            {
                if (exerciseModel.ExerciseData == null)
                {
                    exerciseModel.Discard();
                    (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
                }
                else
                {
                    await exerciseModel.MakeResult();
                    (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
                }
            }
            else
            {
                Continue(obj);
            }
        }

        private async Task OnError(object obj)
        {
            await scanModel.CancelScan(true);
            string msg = null;
            if (Error == 0)
                msg = "当前试卷二维码无法识别，不能查看结果";
            else if (Error == 1)
                msg = "当前试卷二维码无法识别，请检查试卷后重试";
            else if (Error == 2)
                msg = "数据连接异常，请联系服务人员";
            while (true)
            {
                int result = PopupDialog.Show(obj as UIElement, "出现异常", msg, 0, "确定");
                if (result == 0)
                {
                    exerciseModel.Discard();
                    (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
                    break;
                }
            }
        }

        private async Task Finish(object obj)
        {
            if (Error != 0)
                return;
            Update();
            System.Windows.Controls.Page page = obj as System.Windows.Controls.Page;
            FrameworkElement element = page.Resources["ClassDetail"] as FrameworkElement;
            element.DataContext = this;
            while (true)
            {
                int result = -1;
                if (scanModel.Error != null)
                {
                    result = PopupDialog.Show(obj as UIElement, "扫描停止", "扫描仪发生异常，请检查后重试。", 1, "查看结果", "继续扫描");
                }
                else
                {
                    result = PopupDialog.Show(obj as UIElement, "扫描停止",
                        "扫描仪已停止，请添加试卷继续扫描。若已全部扫描，可查看扫描结果。", element, 0, "查看结果", "继续扫描");
                }
                if (result == 0)
                {
                    if (scanModel.PaperCode == null || exerciseModel.ExerciseData == null)
                    {
                        await OnError(obj);
                        break;
                    }
                    await exerciseModel.MakeResult();
                    page.NavigationService.Navigate(new SummaryPage());
                    break;
                }
                else
                {
                    if (base.Continue(obj))
                        break;
                }
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

        private static java.lang.Object GetJavaRuntime()
        {
            JNIEnv env = JNIEnv.ThreadEnv;
            java.lang.Class clazz = env.FindClass("java/lang/Runtime");
            return clazz.Invoke<java.lang.Object>("getRuntime", "()Ljava/lang/Runtime;", new object[0]);
        }

        private java.lang.Object javaRuntime = GetJavaRuntime();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (IsCompleted)
                return;
            Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            Usages.Clear();
            Usages.Add(Tuple.Create("VirtualMemorySize64", currentProcess.VirtualMemorySize64));
            Usages.Add(Tuple.Create("PrivateMemorySize64", currentProcess.PrivateMemorySize64));
            Usages.Add(Tuple.Create("WorkingSet64", currentProcess.WorkingSet64));
            Usages.Add(Tuple.Create("managed heap", System.GC.GetTotalMemory(false)));
            Usages.Add(Tuple.Create("java gc", javaRuntime.Invoke<long>("totalMemory", "()J")));
            Usages.Add(Tuple.Create("java gc max", javaRuntime.Invoke<long>("maxMemory", "()J")));
            foreach (var u in Usages)
                Log.d(u.Item1 + ": " + u.Item2 / 1000000 + " M");
        }

    }
}
