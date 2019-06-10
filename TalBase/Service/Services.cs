using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TalBase.Service
{
    public class Services
    {

        public static HttpMessageHandler MessageHandler { get; set; }

        public static I Get<I>()
        {
            return Base.Service.Services.Get<I>(MessageHandler, new ResultSerializer());
        }
    }
}
