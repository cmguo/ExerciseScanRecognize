using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    class SummaryViewModel : ExerciseViewModel
    {
        public string ExerciseName { get; private set; }
        public int ExceptionCount { get; private set; }
        public ObservableCollection<ExceptionList> Exceptions { get; private set; }

        public RelayCommand ResolveCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public SummaryViewModel() 
        {
            ResolveCommand = new RelayCommand((e) => Resolve(e));
            SubmitCommand = new RelayCommand((e) => Submit(e));
            ExerciseName = exerciseModel.ExerciseData.Title;
            ExceptionCount = exerciseModel.Exceptions.SelectMany(el => el.Exceptions).Count();
            Exceptions = exerciseModel.Exceptions;
        }

        private void Resolve(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ResolvePage());
        }

        private async Task Submit(object obj)
        {
            await exerciseModel.SubmitResult();
            SubmitPage page = new SubmitPage();
            exerciseModel.Clear();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(page);
        }

        protected override bool Continue(object obj)
        {
            bool result = base.Continue(obj);
            if (result)
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanningPage());
            return result;
        }

        internal void FillAll()
        {
            exerciseModel.FillAll();
            ExceptionCount = exerciseModel.Exceptions.SelectMany(el => el.Exceptions).Count();
            RaisePropertyChanged("ExceptionCount");
            Update();
        }
    }
}
