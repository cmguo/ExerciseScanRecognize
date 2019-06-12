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

        public StudentData GetStudent(string id)
        {
            return null;
        }

        public void GetLostPageStudents(Action<StudentData> visitor)
        {
            StudentData student = new StudentData();
            visitor(student);
        }
    }
}
