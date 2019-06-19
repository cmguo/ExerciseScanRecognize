using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Service
{
    public class AccountData
    {

        public string Ticket { get; set; }
        public long Id { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string SchoolName { get; set; }
        //public string[] KickedAppLoginHistoryVoList { get; set; }

    }
}
