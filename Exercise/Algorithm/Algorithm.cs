using com.talcloud.paperanalyze.service.answersheet;
using net.sf.jni4net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class Algorithm
    {

        public Algorithm()
        {
            Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
            AnswerSheetAnalyze.init();
        }

        public QRCodeData GetCode(PageRaw page)
        {
            return analyze<QRCodeData, PageRaw>(AnswerSheetAnalyze.METHOD_QR_CODE_RECOGNIZE, page);
        }

        public AnswerData GetAnswer(PageData page)
        {
            return analyze<AnswerData, PageData>(AnswerSheetAnalyze.METHOD_ANSWER_SHEET_ANALYZE, page);
        }

        private O analyze<O, I>(string method, I input)
        {
            string args = JsonConvert.SerializeObject(input);
            string result = AnswerSheetAnalyze.analyzeAnswerSheet(method, args);
            return JsonConvert.DeserializeObject<O>(result);
        }


    }
}
