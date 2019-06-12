using Account.Service;
using Base.Service;
using Refit;
using System.Threading.Tasks;
using TalBase.Service;

namespace Exercise.Service
{
    [BaseUri("http://interactablet.itest.talcloud.com/app/v1")]
    [MessageHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    public interface IExercise
    {

        [Post("/user/logout")]
        Task<Nothing> Logout();
        Task<ExerciseData> GetExercise(string pageCode);
    }
}
