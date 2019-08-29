using Base.Service;
using Refit;
using System.Threading.Tasks;
using TalBase.Service;

namespace Account.Service
{

    
    [BaseUri("http://homework.idev.talcloud.com/homework/api/v1/answerCardApp")]
    [DelegatingHandler(typeof(LoggingHandler))]
    [DelegatingHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    [Retry(3, 1000)]
    public interface IAccount
    {

        [Post("/login")]
        Task<AccountData> Login([Body] LoginData login);

        [Post("/logout")]
        Task<Nothing> Logout(LogoutData logout);

    }
}
