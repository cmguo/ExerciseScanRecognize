using Base.Misc;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TalBase.Model;

namespace Exercise.Model
{
    public class SchoolModel : ModelBase
    {
        private static SchoolModel s_instance;
        public static SchoolModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new SchoolModel();
                }
                return s_instance;
            }
        }

        public ObservableCollection<ClassInfo> Classes { get; private set; }

        public IList<ClassInfo> AllClasses { get; private set; }

        private IExercise service;
        private SchoolData schoolData;

        public SchoolModel()
        {
            Classes = new ObservableCollection<ClassInfo>();
            service = Services.Get<IExercise>();
        }

        public async Task Refresh()
        {
            Classes.Clear();
            schoolData = await service.getAllClass();
            Classify();
        }

        private void Classify()
        {
            AllClasses = schoolData.ClassInfoList;
            Classes.Clear();
            RaisePropertyChanged("AllClasses");
            foreach (var g in schoolData.StudentInfoList.GroupBy(s => s.ClassId))
            {
                ClassInfo ci = schoolData.ClassInfoList.FirstOrDefault(c => c.ClassId == g.Key);
                if (ci != null)
                    ci.Students = g.ToList();
            }
        }

        public StudentInfo GetStudent(string id)
        {
            StudentInfo student = schoolData.StudentInfoList.FirstOrDefault(s => s.TalNo == id);
            if (student != null && student.AnswerPages == null)
            {
                ClassInfo classData = Classes.FirstOrDefault(c => c.ClassId == student.ClassId);
                if (classData == null)
                {
                    classData = schoolData.ClassInfoList.FirstOrDefault(c => c.ClassId == student.ClassId);
                    if (classData == null)
                        return null;
                    Classes.Add(classData);
                }
                else
                {
                    // notify replace
                    int index = Classes.IndexOf(classData);
                    Classes[index] = Classes[index];
                }
            }
            return student;
        }

        public void GetLostPageStudents(Action<StudentInfo> visitor)
        {
            foreach (StudentInfo s in AllClasses.SelectMany(c =>
            {
                if (!Classes.Contains(c))
                    Classes.Add(c);
                return c.Students.Where(s => s.AnswerPages == null);
            }))
            {
                visitor(s);
            }
        }

        public async Task Save(string path)
        {
            await JsonPersistent.Save(path + "\\school.json", schoolData);
        }

        public async Task Load(string path)
        {
            schoolData = await JsonPersistent.Load<SchoolData>(path + "\\school.json");
            Classify();
        }

        public void Clear()
        {
            schoolData = null;
            Classes.Clear();
        }
    }
}
