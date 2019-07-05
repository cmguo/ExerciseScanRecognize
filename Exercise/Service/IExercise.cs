using Account.Service;
using Base.Service;
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
        Task<ExerciseData> GetExercise(string paperId);

        [Post("/getTempHomeWorkIdTempId")]
        Task<StringData> GetSubmitId(SubmitPrepare prepare);

        [Post("/answerCardCropImageNotify")]
        Task<Nothing> CompleteSubmit(SubmitComplete complete);

        [Post("/uploadResult")]
        Task<Nothing> Submit(SubmitData data);

        [Post("/batchGeneratePresignedUrl")]
        Task<Dictionary<string, string>> GeneratePresignedUrls(GenUriData names);

        [Post("/getScanList")]
        Task<HistoryData> getRecords(HistoryData.Range range);

        [Post("/updateRecord")]
        Task<Nothing> updateRecord(HistoryData.Record record);
    }
}
