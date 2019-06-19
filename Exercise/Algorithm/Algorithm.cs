using Base.Misc;
using Base.Protocol;
using com.talcloud.paperanalyze.service.answersheet;
using net.sf.jni4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class Algorithm
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public Algorithm()
        {
            Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
            AnswerSheetAnalyze.init();
            //Test();
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
            string args = JsonConvert.SerializeObject(input, settings);
            string result = AnswerSheetAnalyze.analyzeAnswerSheet(method, args);
            return JsonConvert.DeserializeObject<Result<O>>(result, settings).Data;
        }

        public async void Test()
        {
            string json = @"C:\Users\Brandon\source\repos\DotNet\Exercise\long_text_2019-06-18-15-12-30.txt";
            string jpg = @"C:\Users\Brandon\source\repos\DotNet\Exercise\answersheet.jpg";
            PageRaw pagew = new PageRaw();
            pagew.ImgBytes = await UriFetcher.GetDataAsync(new Uri(jpg));
            QRCodeData code = GetCode(pagew);
            PageData page = await JsonPersistent.Load<PageData>(json);
            page.ImgBytes = pagew.ImgBytes;
            AnswerData answer = GetAnswer(page);
        }

    }
}
