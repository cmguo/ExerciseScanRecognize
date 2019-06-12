using Refit;
using System;
using System.Net.Http;

namespace Base.Service
{
    public class Services
    {

        public static I Get<I>()
        {
            BaseUriAttribute uriAttr = (BaseUriAttribute)typeof(I).GetCustomAttributes(typeof(BaseUriAttribute), true)[0];
            MessageHandlerAttribute mhdlAttr = (MessageHandlerAttribute)typeof(I).GetCustomAttributes(typeof(MessageHandlerAttribute), true)[0];
            ContentSerializer cserAttr = (ContentSerializer)typeof(I).GetCustomAttributes(typeof(ContentSerializer), true)[0];
            RetryAttribute[] retryAttr = (RetryAttribute[])typeof(I).GetCustomAttributes(typeof(RetryAttribute), true);
            RefitSettings settings = new RefitSettings();
            settings.ContentSerializer = (IContentSerializer) Activator.CreateInstance(cserAttr.Value);
            HttpMessageHandler handler = (HttpMessageHandler) Activator.CreateInstance(mhdlAttr.Value);
            if (retryAttr.Length > 0)
            {
                handler = new RetryHandler(handler, retryAttr[0]);
            }
            return RestService.For<I>(new HttpClient(handler) { BaseAddress = new Uri(uriAttr.Value) }, settings);
        }
    }
}
