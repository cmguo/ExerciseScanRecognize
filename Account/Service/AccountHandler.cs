using Account.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Service
{
    class AccountHandler : HttpClientHandler
    {
        public AccountHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string ticket = AccountModel.Instance.Account.Ticket; ;
            request.Headers.TryAddWithoutValidation("ticket", ticket);
            HttpResponseMessage resp = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return resp;
        }
    }
}
