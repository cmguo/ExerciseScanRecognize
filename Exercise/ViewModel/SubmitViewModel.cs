using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Windows;
using System.Windows.Input;
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

        private async void Close(object obj)
        {
            int result = PopupDialog.Show(obj as UIElement, "退出软件", "扫描结果上传中，退出后，扫描结果将放弃，确认退出吗？", 0, "退出", "取消");
            if (result == 0)
            {
                await submitModel.Cancel(Task);
                Window.GetWindow((obj as ExecutedRoutedEventArgs).OriginalSource as UIElement).Close();
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
