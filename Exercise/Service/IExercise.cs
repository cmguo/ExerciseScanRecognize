using Base.Service;
using Refit;
using System.Threading.Tasks;
using TalBase.Service;

namespace Exercise.Service
{
    [BaseUri("http://interactablet.itest.talcloud.com/app/v1")]
    public interface IExercise
    {

        [Post("/user/logout")]
        Task<Nothing> Logout();

    }
}
