using Account.Model;
using Base.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Service
{
    public class AccountHandler : HttpClientHandler
    {
        public AccountHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string ticket = AccountModel.Instance.Account.Ticket;
            if (ticket != null)
                request.Headers.TryAddWithoutValidation("ticket", ticket);
            HttpResponseMessage resp = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                throw new HttpResponseException(resp.StatusCode, resp.ReasonPhrase);
            return resp;
        }
    }
}
