using Refit;
using System;
using System.Net.Http;

namespace Base.Service
{
    public class Services
    {

        public static I Get<I>(HttpMessageHandler httpMessageHandler, IContentSerializer contentSerializer)
        {
            BaseUriAttribute uriAttr = (BaseUriAttribute) typeof(I).GetCustomAttributes(typeof(BaseUriAttribute), true)[0];
            RefitSettings settings = new RefitSettings();
            settings.ContentSerializer = contentSerializer; 
            return RestService.For<I>(new HttpClient(httpMessageHandler) { BaseAddress = new Uri(uriAttr.Value) }, settings);
        }
    }
}
