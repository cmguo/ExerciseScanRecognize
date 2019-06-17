using Account.Service;
using Base.Service;
using Exercise.Model;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using TalBase.Service;

namespace Exercise.Service
{
    [BaseUri("http://homework.idev.talcloud.com/homework/api/v1")]
    [MessageHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    public interface IExercise
    {

        [Post("/answerCardApp/getAllClass")]
        Task<SchoolData> getAllClass();

        [Post("/answerCardApp/getAllClass")]
        Task<ExerciseData> GetExercise(string pageCode);

        [Post("/user/logout")]
        Task<Nothing> Submit(SubmitData data);

        [Post("/answerCardApp/batchGeneratePresignedUrl")]
        Task<Dictionary<string, string>> GeneratePresignedUrls(GenUriData names);

        [Post("/answerCardApp/batchGeneratePresignedUrl")]
        Task<RecordData> getRecords(int page);
    }
}
