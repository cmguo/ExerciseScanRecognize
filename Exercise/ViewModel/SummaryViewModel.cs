using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    class SummaryViewModel : ScanViewModel
    {
        public class ClassDetail
        {
            public string ClassName { get; set; }
            public int StudentCount { get; set; }
            public int ResultCount { get; set; }
        }

        public string ExerciseName { get; private set; }
        public int StudentCount { get; private set; }
        public int ExceptionCount { get; private set; }
        public List<ClassDetail> ClassDetails { get; private set; }
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
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.Any(p => p.StudentCode == null)).Count();
            ExceptionCount = exerciseModel.Exceptions.SelectMany(el => el.Exceptions).Count();
            ClassDetails = schoolModel.Classes.Select(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                StudentCount = c.Students.Count(),
                ResultCount = c.Students.Where(s => s.AnswerPages.Any(p => p.StudentCode == null)).Count(),
            }).ToList();
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

        protected override void Continue(object obj)
        {
            base.Continue(obj);
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanningPage());
        }

    }
}
