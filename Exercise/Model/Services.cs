using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
{
    class Services
    {

        public static I Get<I>()
        {
            BaseUriAttribute uriAttr = (BaseUriAttribute) typeof(I).GetCustomAttributes(typeof(BaseUriAttribute), true)[0];
            RefitSettings settings = new RefitSettings();
            settings.ContentSerializer = new ResultSerializer(); 
            return RestService.For<I>(new HttpClient(new AccountHandler()) { BaseAddress = new Uri(uriAttr.Value) }, settings);
        }
    }
}
