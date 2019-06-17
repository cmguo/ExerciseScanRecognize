using Base.Mvvm;
using Exercise.Model;
using MyToolkit.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Exercise.ViewModel
{
    class ScanResultViewModel : ViewModelBase
    {
        public class ClassDetail
        {
            public string ClassName { get; set; }
            public int StudentCount { get; set; }
            public int ResultCount { get; set; }
        }

        public int StudentCount { get; private set; }
        public int ExceptionCount { get; private set; }
        public List<ClassDetail> ClassDetails { get; private set; }

        public RelayCommand ContinueScanCommand { get; set; }

        public RelayCommand HandleExceptionCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public ScanResultViewModel() 
        {
            ContinueScanCommand = new RelayCommand((e) => ContinueScan(e));
            HandleExceptionCommand = new RelayCommand((e) => HandleException(e));
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.IndexOf(null) < 0).Count();
            ExceptionCount = exerciseModel.Exceptions.SelectMany(el => el.Exceptions).Count();
            ClassDetails = schoolModel.Classes.Select(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                StudentCount = c.Students.Count(),
                ResultCount = c.Students.Where(s => s.AnswerPages.IndexOf(null) < 0).Count(),
            }).ToList();
        }

        private void HandleException(object obj)
        {
            (obj as NavigationWindow).Navigate((Page) null);
        }

        private void ContinueScan(object obj)
        {
            (obj as NavigationWindow).Navigate((Page) null);
        }
    }
}
