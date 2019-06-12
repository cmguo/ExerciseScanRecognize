using Base.Service;
using Refit;
using System.Threading.Tasks;
using TalBase.Service;

namespace Account.Service
{
    [BaseUri("http://interactablet.itest.talcloud.com/app/v1")]
    [MessageHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    [Retry(3, 1000)]
    public interface IAccount
    {

        [Post("/user/login")]
        Task<AccountData> Login([Body] LoginData login);

        [Post("/user/logout")]
        Task<Nothing> Logout();

    }
}
