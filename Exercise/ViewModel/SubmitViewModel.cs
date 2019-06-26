using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using Panuon.UI;
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
        }

        public override void Release()
        {
            base.Release();
            Task.PropertyChanged -= Task_PropertyChanged; ;
        }

        private void Retry(object obj)
        {
            BackgroudWork.Execute(() => submitModel.Submit(Task));
        }

        private void Return(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
        }

        private void Close(object obj)
        {
            bool? isConfirm = PUMessageBox.ShowConfirm("扫描结果上传中，退出后，扫描结果将放弃，确认退出吗？", "提示");
            if (isConfirm != null && isConfirm.Value)
            {
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
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
