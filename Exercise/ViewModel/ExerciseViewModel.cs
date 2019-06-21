using Base.Mvvm;
using Exercise.Model;
using MyToolkit.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    public class ExerciseViewModel : ScanViewModel
    {
        public class ClassDetail
        {
            public string ClassName { get; set; }
            public int StudentCount { get; set; }
            public int ResultCount { get; set; }
        }

        #region Properties

        public int StudentCount { get; private set; }
        public ObservableCollection<ClassDetail> ClassDetails { get; private set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public ExerciseViewModel() 
        {
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.Any(p => p.StudentCode == null)).Count();
            ClassDetails = schoolModel.Classes.Select2(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                StudentCount = c.Students.Count(),
                ResultCount = c.Students.Where(s => s.AnswerPages.Any(p => p.StudentCode == null)).Count(),
            });
        }

    }
}
