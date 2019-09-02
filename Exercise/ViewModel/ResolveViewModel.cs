using Base.Misc;
using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Threading.Tasks;
using System.Windows;
using TalBase.View;
using static Exercise.Model.ExerciseModel;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.ViewModel
{
    class ResolveViewModel : ExerciseViewModel
    {

        private static readonly Logger Log = Logger.GetLogger<ResolveViewModel>();

        #region Properties

        private ExceptionList _SelectedExceptionList;
        public ExceptionList SelectedExceptionList
        {
            get { return _SelectedExceptionList; }
            set { _SelectedExceptionList = value; RaisePropertyChanged("SelectedExceptionList"); }
        }

        private Exception _SelectedException;
        public Exception SelectedException
        {
            get { return _SelectedException; }
            set
            {
                _SelectedException = value;
                RaisePropertyChanged("SelectedException");
            }
        }

        private object _Selection;
        public object Selection
        {
            get => _Selection;
            set
            {
                if (value == _Selection)
                    return;
                _Selection = value;
                RaisePropertyChanged("Selection");
                if (value is Exception)
                {
                    SelectedException = value as Exception;
                    SelectedExceptionList = exerciseModel.Exceptions.Find(l => l.Exceptions.Contains(value as Exception));
                }
                else
                {
                    SelectedException = null;
                    SelectedExceptionList = value as ExceptionList;
                }
            }
        }

        #endregion

        #region Commands

        public RelayCommand RescanCommand { get; private set; }
        public RelayCommand IgnoreCommand { get; set; }
        public RelayCommand RemovePageCommand { get; set; }
        public RelayCommand RemoveStudentCommand { get; set; }
        public RelayCommand ResolveCommand { get; set; }
        public RelayCommand IgnoreListCommand { get; set; }
        public RelayCommand RemovePageListCommand { get; set; }

        public RelayCommand ReturnCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;
        private int exceptionCount;

        public ResolveViewModel()
        {
            RescanCommand = new RelayCommand((e) => Rescan(e));
            IgnoreCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.Ignore));
            RemovePageCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.RemovePage));
            RemoveStudentCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.RemoveStudent));
            ResolveCommand = new RelayCommand((e) => Resolve(e, SelectedException, ResolveType.Resolve));
            IgnoreListCommand = new RelayCommand((e) => Resolve(e, SelectedExceptionList, ResolveType.Ignore));
            RemovePageListCommand = new RelayCommand((e) => Resolve(e, SelectedExceptionList, ResolveType.RemovePage));
            CloseMessage = "本次扫描结果未上传，" + CloseMessage;
            exerciseModel.BeforeReplacePage += ExerciseModel_BeforeReplacePage;
            ReturnCommand = new RelayCommand((o) => Return(o));
            exceptionCount = ExceptionCount;
            HistoryModel.Instance.BeginDuration(HistoryModel.DurationType.Resolve);
        }

        public override void Release()
        {
            Update();
            HistoryModel.Instance.EndDuration(exceptionCount - ExceptionCount);
            base.Release();
            exerciseModel.BeforeReplacePage -= ExerciseModel_BeforeReplacePage;
        }

        public void InitSelection()
        {
            if (Exceptions.Count > 0)
                Selection = Exceptions[0].Exceptions[0];
        }

        private async Task Rescan(object obj)
        {
            if (!Check(obj))
                return;
            Exception ex = SelectedException;
            try
            {
                await exerciseModel.ScanOne(ex);
            }
            catch (System.Exception e)
            {
                Log.w("Rescan", e);
                return;
            }
            if (ex.Page != null)
            {
                TalToast.Show("X该试卷仍无法识别，请检查后重新扫描");
            }
            else
            {
                TalToast.Show("该份试卷的异常已处理完成");
            }
            if (Exceptions.Count == 0)
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

        private void ExerciseModel_BeforeReplacePage(object sender, ReplacePageEventArgs e)
        {
            if (e.Old != SelectedException.Page)
            {
                int result = PopupDialog.Show(Application.Current.MainWindow,"替换试卷确认", "您放入的学生试卷已经有扫描结果，确认替换吗？", 0, "确认", "取消");
                e.Cancel = result == 1;
            }
        }

        private async Task Resolve(object obj, ExerciseModel.Exception exception, ResolveType type)
        {
            string title = null;
            string message = null;
            string btn = null;
            switch (type)
            {
                case ResolveType.Ignore:
                case ResolveType.RemovePage:
                    title = "忽略异常";
                    btn = "忽略";
                    switch (SelectedException.Type)
                    {
                        case ExceptionType.NoStudentCode:
                        case ExceptionType.StudentCodeMissMatch:
                            message = "忽略后，该张试卷所有题目作答将无法统计，确认忽略吗？";
                            break;
                        case ExceptionType.PageLost:
                            message = "忽略后，该张试卷无法还原，确认忽略吗？";
                            break;
                        case ExceptionType.AnswerException:
                        case ExceptionType.CorrectionException:
                            title = "设置异常题目为0分";
                            btn = "确定";
                            message = "确认要将本页所有的异常题目设置为0分吗？";
                            break;
                        default:
                            //Todo
                            message = "忽略后，该张试卷无法复原，确认忽略吗？";
                            break;
                    }
                    break;
                case ResolveType.RemoveStudent:
                    title = "删除学生";
                    btn = "删除";
                    message = "删除后该生本次考试将被放弃，不计入统计，确认删除吗？";
                    break;
            }
            if (title != null)
            {
                int n = PopupDialog.Show(obj as FrameworkElement, title, message, 0, btn, "取消");
                if (n != 0)
                    return;
            }
            Exception ex = SelectedException;
            exerciseModel.Resolve(exception, type);
            if (Exceptions.Count == 0)
            {
                await exerciseModel.Save();
                TalToast.Show("异常已全部处理完成");
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
            else if (ex.Page == null)
            {
                string toast = null;
                switch (type)
                {
                    case ResolveType.Ignore:
                    case ResolveType.RemovePage:
                        toast = "该份试卷的异常已忽略";
                        break;
                    case ResolveType.Resolve:
                        toast = "该份试卷的异常已处理完成";
                        break;
                }
                TalToast.Show(toast);
            }
        }

        private void Resolve(object obj, ExerciseModel.ExceptionList list, ResolveType type)
        {
            int n = PopupDialog.Show(obj as FrameworkElement, "忽略异常",
                "确认忽略以上异常？", 0, "确认", "取消");
            if (n != 0)
                return;
            exerciseModel.Resolve(list, type);
            if (Exceptions.Count == 0)
            {
                TalToast.Show("异常已全部处理完成");
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
            }
        }

        private async Task Return(object obj)
        {
            await exerciseModel.CancelScanOne();
            await exerciseModel.Save();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
        }

    }
}
