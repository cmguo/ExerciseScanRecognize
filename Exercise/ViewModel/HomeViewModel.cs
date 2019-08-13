using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Threading.Tasks;
using TalBase.Utils;
using System.Windows.Navigation;
using TalBase.View;
using System.Windows;
using static Exercise.Service.HistoryData;
using static Exercise.Model.SubmitModel;
using System;
using Assistant;

namespace Exercise.ViewModel
{
    public class HomeViewModel : ScanViewModel
    {
        public string[] SourceList { get; internal set; }

        #region Commands

        public RelayCommand CheckLocalCommand { get; set; }
        public RelayCommand StartCommand { get; set; }
        public RelayCommand HistroyCommand { get; set; }

        #endregion

        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private ScanModel scanModel = ScanModel.Instance;

        public HomeViewModel()
        {
            CheckLocalCommand = new RelayCommand((e) => CheckLocal(e));
            StartCommand = new RelayCommand((e) => Start(e));
            HistroyCommand = new RelayCommand((e) => History(e));
            SourceList = scanModel.SourceList;
            AppStatus.Instance.Busy = false;
        }

        public override void Release()
        {
            AppStatus.Instance.Busy = true;
        }

        private static bool checkLocal = true;

        private async Task CheckLocal(object obj)
        {
            if (!checkLocal)
                return;
            checkLocal = false;
            Record record = await HistoryModel.Instance.Load();
            if (record != null)
            {
                SubmitTask task = await SubmitModel.Instance.Load(record.LocalPath);
                string msg = task == null ? "因客户端非正常关闭，上次读卷尚未完成，是否继续处理？" 
                    : "因客户端非正常关闭，上次读卷结果尚未完成，是否继续处理？";
                int result = PopupDialog.Show(obj as UIElement, "提示", msg, 0, "继续处理", "忽略");
                if (result == 0)
                {
                    try
                    {
                        await ExerciseModel.Instance.Load(record.LocalPath);
                    }
                    catch
                    {
                        ExerciseModel.Instance.Clear();
                        throw;
                    }
                    (obj as System.Windows.Controls.Page).NavigationService.Navigate(new SummaryPage());
                }
                else
                {
                    HistoryModel.Instance.Remove(record);
                }
            }
        }

        private async Task Start(object obj)
        {
            NetWorkManager.CheckNetWorkAvailable("当前电脑无网络连接，请检查后再开始扫描。");
            await exerciseModel.NewTask();
            if (base.Continue(obj))
            {
                NavigationService navigationService = (obj as System.Windows.Controls.Page).NavigationService;
                navigationService.Navigate(new ScanningPage());
            }
            else
            {
                exerciseModel.Discard();
            }
        }

        private void History(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HistoryPage());
        }

    }
}
