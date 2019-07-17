using Exercise.Model;
using Exercise.Service;
using Exercise.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TalBase.View;

namespace Exercise.ViewModel
{
    class SchoolViewModel : ExerciseViewModel
    {

        #region Properties

        public IList<ClassInfo> Classes { get; set; }

        private ClassInfo _SelectedClass;
        public ClassInfo SelectedClass
        {
            get => _SelectedClass;
            set
            {
                _SelectedClass = value;
                StudentFilter = null;
            }
        }

        public IList<StudentInfo> FilteredStudents { get; private set; }
        private string _StudentFilter;
        public string StudentFilter
        {
            get => _StudentFilter;
            set
            {
                _StudentFilter = value;
                RaisePropertyChanged("StudentFilter");
                FilteredStudents = SelectedClass == null 
                    ? null : ((value == null || value.Length == 0)
                    ? SelectedClass.Students.OrderBy(s => s.ToString()).ToList()
                    : SelectedClass.Students.Where(s => s.ToString().Contains(value)).OrderBy(s => s.ToString()).ToList());
                RaisePropertyChanged("FilteredStudents");
            }
        }

        #endregion


        private SchoolModel schoolModel = SchoolModel.Instance;

        public SchoolViewModel()
        {
            Classes = schoolModel.AllClasses.OrderBy(c => c.ClassName).ToList();
        }

    }
}
