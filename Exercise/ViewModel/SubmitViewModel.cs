using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.ComponentModel;
using System.Windows;
using TalBase.View;
using TalBase.ViewModel;

namespace Exercise.ViewModel
{
    class SubmitViewModel : ViewModelBase
    {
        public string ExerciseName { get; private set; }
        public SubmitModel.SubmitTask Task { get; private set; }
        public int Percent { get; private set; }

        #region Commands

        public RelayCommand RetryCommand { get; private set; }
        public RelayCommand ReturnCommand { get; private set; }
        public RelayCommand DiscardCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        #endregion

        private SubmitModel submitModel = SubmitModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public SubmitViewModel()
        {
            ExerciseName = exerciseModel.ExerciseData.Title;
            Task = submitModel.SubmitTasks[exerciseModel.SavePath];
            Task.PropertyChanged += Task_PropertyChanged;
            RetryCommand = new RelayCommand((e) => Retry(e));
            ReturnCommand = new RelayCommand((e) => Return(e));
            DiscardCommand = new RelayCommand((e) => Discard(e));
            CloseCommand = new RelayCommand((e) => Close(e));
        }

        public override void Release()
        {
            base.Release();
            Task.PropertyChanged -= Task_PropertyChanged; ;
        }

        private void Retry(object obj)
        {
            submitModel.Submit(Task);
        }

        private void Return(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
        }

        private void Discard(object obj)
        {
            int result = PopupDialog.Show(obj as UIElement, "放弃扫描任务", "放弃后，本次扫描结果将作废，确认放弃么？", 0, "放弃本次扫描", "取消");
            if (result == 0)
            {
                submitModel.Remove(Task);
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
        }

        private async void Close(object obj)
        {
            if (Task.Status != SubmitModel.TaskStatus.Submiting)
                return;
            CancelEventArgs e = obj as CancelEventArgs;
            int result = PopupDialog.Show(obj as UIElement, "退出软件", "扫描结果上传中，退出后，扫描结果将放弃，确认退出吗？", 0, "退出", "取消");
            if (result == 0)
            {
                await submitModel.Cancel(Task);
                submitModel.Remove(Task);
                Application.Current.MainWindow.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Task_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Finish")
            {
                Percent = Task.Finish * 100 / Task.Total;
                RaisePropertyChanged("Percent");
            }
        }
    }
}
