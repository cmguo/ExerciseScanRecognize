using Base.Misc;
using Base.Service;
using Exercise.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exercise.Model
{
    class SubmitModel
    {

        private static SubmitModel s_instance;
        public static SubmitModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new SubmitModel();
                }
                return s_instance;
            }
        }

        public Dictionary<string, SubmitData> SubmitTasks { get; private set; }

        private IExercise service;

        public SubmitModel()
        {
            SubmitTasks = new Dictionary<string, SubmitData>();
            service = Services.Get<IExercise>();
        }

        public async Task Save(string path, ExerciseData exercise, List<StudentInfo> students)
        {
            IList<SubmitData.AnswerInfo> data = students.Select(s => new SubmitData.AnswerInfo()
            {
                StudentId = s.StudentNo,
                PageInfo = s.AnswerPages.Where(p => p != null).Select(p => p.Answer).ToList()
            }).ToList();
            SubmitData sdata = new SubmitData() { HomeworkId = exercise.exerciseId, PaperId = exercise.exerciseId, Data = data };
            SubmitTasks[path] = sdata;
            await JsonPersistent.Save(path + "\\submit.json", sdata);
        }

        public async Task Submit(string path)
        {
            SubmitData sdata = SubmitTasks[path];
            if (sdata == null)
            {
                sdata = await JsonPersistent.Load<SubmitData>(path + "\\submit.json");
                SubmitTasks[path] = sdata;
            }
            await service.Submit(sdata);
            List<string> pageNames = sdata.Data.SelectMany(s => s.PageInfo.Select(p => p.ImageName)).ToList();
            Dictionary<string, string> pageUrls = await service.GeneratePresignedUrls(new GenUriData() { ObjectNameList = pageNames });
            HttpClient hc = new HttpClient();
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "image/jpg");
            foreach (var p in pageUrls)
            {
                FileStream fs = new FileStream(path + "\\" + p.Key, FileMode.Open, FileAccess.Read);
                StreamContent content = new StreamContent(fs);
                content.Headers.Add("Content-Type", "image/jpg");
                var response = await hc.PutAsync(p.Value, content);
                if (response.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                    throw new HttpResponseException(response.StatusCode, response.ReasonPhrase);
            }
            SubmitTasks.Remove(path);
            Directory.Delete(path, true);
        }

    }

}
