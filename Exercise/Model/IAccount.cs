using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
{
    [BaseUri("http://interactablet.itest.talcloud.com/app/v1")]
    interface IAccount
    {

        [Post("/user/login")]
        Task<Account> Login([Body] Login login);

        [Post("/user/logout")]
        Task<Nothing> Logiut();

    }
}
