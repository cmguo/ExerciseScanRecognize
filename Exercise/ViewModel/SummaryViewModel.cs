using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using MyToolkit.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    class SummaryViewModel : ViewModelBase
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

        public RelayCommand ContinueCommand { get; set; }

        public RelayCommand ResolveCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public SummaryViewModel() 
        {
            ContinueCommand = new RelayCommand((e) => Continue(e));
            ResolveCommand = new RelayCommand((e) => Resolve(e));
            ExerciseName = "三角函数";// exerciseModel.ExerciseData.ExerciseName;
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.IndexOf(null) < 0).Count();
            ExceptionCount = exerciseModel.Exceptions.SelectMany(el => el.Exceptions).Count();
            ClassDetails = schoolModel.Classes.Select(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                StudentCount = c.Students.Count(),
                ResultCount = c.Students.Where(s => s.AnswerPages.IndexOf(null) < 0).Count(),
            }).ToList();
            Exceptions = exerciseModel.Exceptions;
        }

        private void Resolve(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ResolvePage());
        }

        private async Task Continue(object obj)
        {
            await scanModel.Scan();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanPage());
        }

    }
}
