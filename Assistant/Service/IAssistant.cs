using Account.Service;
using Assistant.Fault;
using Base.Service;
using Refit;
using System.Threading.Tasks;
using TalBase.Service;

namespace Assistant.Service
{

    
    [BaseUri("http://homework.idev.talcloud.com/homework/api/v1/answerCardApp")]
    [DelegatingHandler(typeof(LoggingHandler))]
    [DelegatingHandler(typeof(AccountHandler))]
    [ContentSerializer(typeof(ResultSerializer))]
    [Retry(3, 1000)]
    public interface IAssistant
    {

        [Post("/saveClientCrashLog")]
        Task<ReportResult> FaultReport([Body] CrashReport report);

    }
}
