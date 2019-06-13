using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Service
{
    public class SchoolData
    {
        public IList<ClassData> classes { get; set; }
        public IList<StudentData> students { get; set; }
    }
}
