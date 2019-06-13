using Exercise.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IList<ClassData> Classes { get; private set; }

        private SchoolData schoolData;

        public SchoolModel()
        {
            Classes = new List<ClassData>();
        }

        public StudentData GetStudent(string id)
        {
            StudentData student = schoolData.students.FirstOrDefault(s => s.number == id);
            if (student != null && student.AnswerPages == null)
            {
                if (Classes.FirstOrDefault(c => c.id == student.clsid) == null)
                {
                    ClassData classData = schoolData.classes.FirstOrDefault(c => c.id == student.clsid);
                    if (classData == null)
                        return null;
                    Classes.Add(classData);
                }
            }
            return student;
        }

        public void GetLostPageStudents(Action<StudentData> visitor)
        {
            IEnumerable<StudentData> students = schoolData.students.Where(
                s => Classes.FirstOrDefault(c => c.id == s.clsid) != null && (s.AnswerPages == null || s.AnswerPages.Contains(null)));
            foreach (StudentData s in students)
                visitor(s);
        }
    }
}
