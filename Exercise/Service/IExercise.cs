using Account.Service;
using Base.Service;
using Exercise.Model;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using TalBase.Service;

namespace Exercise.Service
{
    [BaseUri("http://homework.idev.talcloud.com/homework/api/v1/answerCardApp")]
    [MessageHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    public interface IExercise
    {

        [Get("/getAllClass")]
        Task<SchoolData> getAllClass();

        [Get("/getAnswersheetData")]
        Task<ExerciseData> GetExercise(string pagerId);

        [Get("/getTempHomeWorkIdTempId")]
        Task<string> GetSubmitId();

        [Post("/uploadResult")]
        Task<Nothing> Submit(SubmitData data);

        [Post("/batchGeneratePresignedUrl")]
        Task<Dictionary<string, string>> GeneratePresignedUrls(GenUriData names);

        [Get("/batchGeneratePresignedUrl")]
        Task<RecordData> getRecords(int page);
    }
}
