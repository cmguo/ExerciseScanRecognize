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

        private SubmitModel submitModel = SubmitModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;

        public SubmitViewModel()
        {
            ExerciseName = exerciseModel.ExerciseData.ExerciseName;
            string exerciseId = null;
            Task = submitModel.SubmitTasks[exerciseId];
        }
    }
}
