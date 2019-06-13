using Exercise.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Service
{
    public class StudentData
    {
        public IList<Page> AnswerPages { get; set; }

        public string id { get; set; }
        public string number { get; set; }
        public string clsid { get; set; }
    }
}
