using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
{
    public class Account
    {

        [JsonProperty(PropertyName = "id")]
        public String UserId;

        [JsonProperty(PropertyName = "name")]
        public String UserName;

        [JsonProperty(PropertyName = "password")]
        public String Password;

        [JsonProperty(PropertyName = "ticket")]
        public String Ticket;

    }
}
