using Base.Helpers;
using Exercise.Model;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    ? SelectedClass.Students.Sort().ToList()
                    : SelectedClass.Students.Where(s => s.ToString().Contains(value)).OrderBy(s => s.ToString()).ToList());
                RaisePropertyChanged("FilteredStudents");
            }
        }

        #endregion


        private SchoolModel schoolModel = SchoolModel.Instance;

        public SchoolViewModel()
        {
            ClassInfo all = new ClassInfo()
            {
                ClassName = "全部班级",
                Students = schoolModel.AllClasses.SelectMany(c => c.Students).ToList()
            };
            Classes = Enumerable.Repeat(all, 1).Concat(schoolModel.AllClasses
                .Sort()).ToList();
            SelectedClass = all;
        }

    }
}
