using Base.Misc;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
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

        public IList<ClassInfo> Classes { get; private set; }

        private IExercise service;
        private SchoolData schoolData;

        public SchoolModel()
        {
            Classes = new List<ClassInfo>();
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
                if (Classes.FirstOrDefault(c => c.ClassId == student.ClassId) == null)
                {
                    ClassInfo classData = schoolData.ClassInfoList.FirstOrDefault(c => c.ClassId == student.ClassId);
                    if (classData == null)
                        return null;
                    Classes.Add(classData);
                }
            }
            return student;
        }

        public void GetLostPageStudents(Action<StudentInfo> visitor)
        {
            IEnumerable<StudentInfo> students = schoolData.StudentInfoList.Where(
                s => Classes.FirstOrDefault(c => c.ClassId == s.ClassId) != null && (s.AnswerPages == null || s.AnswerPages.Contains(null)));
            foreach (StudentInfo s in students)
                visitor(s);
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
            Classes = null;
        }
    }
}
