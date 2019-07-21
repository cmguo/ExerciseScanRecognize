using Exercise.Model;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exercise.ViewModel
{
    class SchoolViewModel : ExerciseViewModel, IComparer<string>
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
            ClassInfo all = new ClassInfo()
            {
                ClassName = "全部班级",
                Students = schoolModel.AllClasses.SelectMany(c => c.Students).ToList()
            };
            Classes = Enumerable.Repeat(all, 1).Concat(schoolModel.AllClasses
                .OrderBy(c => c.ClassName, this)).ToList();
            SelectedClass = all;
        }

        private readonly string hans = "一二三四五六七八九十初高";

        public int Compare(string x, string y)
        {
            int i = 0;
            for (; i < x.Length && i < y.Length; ++i)
            {
                if (Char.IsDigit(x[i]) && Char.IsDigit(y[i]))
                {
                    int ix = i + 1;
                    while (ix < x.Length && Char.IsDigit(x[ix]))
                        ++ix;
                    int nx = Int32.Parse(x.Substring(i, ix - i));
                    int iy = i + 1;
                    while (iy < y.Length && Char.IsDigit(y[iy]))
                        ++iy;
                    int ny = Int32.Parse(y.Substring(i, iy - i));
                    if (nx == ny)
                    {
                        i = ix - 1;
                        continue;
                    }
                    return nx - ny;
                }
                else if (x[i] == y[i])
                {
                    continue;
                }
                else if (hans.Contains(x[i]) && hans.Contains(y[i]))
                {
                    return hans.IndexOf(x[i]) - hans.IndexOf(y[i]);
                }
                else
                {
                    return x[i] - y[i];
                }
            }
            return x.Length - y.Length;
        }
    }
}
