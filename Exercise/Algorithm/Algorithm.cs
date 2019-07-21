﻿using Base.Misc;
using Base.Mvvm;
using com.talcloud.paperanalyze.service.answersheet;
using net.sf.jni4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class Algorithm
    {
        private static readonly Logger Log = Logger.GetLogger<Algorithm>();

        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public Algorithm()
        {
            BackgroudWork.Execute(() => Task.Run(() =>
            {
                Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
                AnswerSheetAnalyze.init();
                //Test();
            }));
        }

        public QRCodeData GetCode(PageRaw page)
        {
            QRCodeData code = Analyze<QRCodeData, PageRaw>(AnswerSheetAnalyze.METHOD_QR_CODE_RECOGNIZE, page);
            if (code.PaperInfo == null || code.PaperInfo.Length == 0)
                throw new NullReferenceException("试卷二维码未识别");
            if (code.StudentInfo == "")
                code.StudentInfo = null;
            return code;
        }

        [HandleProcessCorruptedStateExceptions]
        public AnswerData GetAnswer(PageData page)
        {
            return Analyze<AnswerData, PageData>(AnswerSheetAnalyze.METHOD_ANSWER_SHEET_ANALYZE, page);
        }

        private O Analyze<O, I>(string method, I input)
        {
            try
            {
                string args = JsonConvert.SerializeObject(input, settings);
                string result = AnswerSheetAnalyze.analyzeAnswerSheet(method, args);
                Result<O> output = JsonConvert.DeserializeObject<Result<O>>(result, settings);
                if (output.Code != 0)
                    throw new AlgorithmException(output.Code, output.Message);
                return output.Data;
            }
            catch (Exception e)
            {
                Log.w(e);
                throw new Exception("识别算法异常", e);
            }
        }

        public async void Test()
        {
            string json = @"C:\Users\Brandon\source\repos\DotNet\Exercise\0620\long_text_2019-06-20-16-53-51.txt";
            string jpg = @"C:\Users\Brandon\source\repos\DotNet\Exercise\0620\3.jpg";
            using (FileStream fs = new FileStream(jpg, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream ms = new MemoryStream((int) fs.Length))
            {
                await fs.CopyToAsync(ms);
                PageRaw pagew = new PageRaw();
                pagew.ImgBytes = ms.GetBuffer();
                QRCodeData code = GetCode(pagew);
                PageData page = await JsonPersistent.Load<PageData>(json);
                page.ImgBytes = pagew.ImgBytes;
                AnswerData answer = GetAnswer(page);
            }
        }

    }
}
