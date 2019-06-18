using Exercise.Model;
using MyToolkit.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.ViewModel
{
    class SubmitViewModel : ViewModelBase
    {
        public string ExerciseName { get; private set; }
        public SubmitModel.SubmitTask Task { get; private set; }
        public int Percent { get; private set; }

        private SubmitModel submitModel = SubmitModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public SubmitViewModel()
        {
            ExerciseName = exerciseModel.ExerciseData.ExerciseName;
            Task = submitModel.SubmitTasks[exerciseModel.SavePath];
            Task.PropertyChanged += Task_PropertyChanged; ;
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
