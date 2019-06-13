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

        public enum SubmitStatus
        {
            Scaning,
            Exception,
            Ready, 
            Submitting, 
        }

        public class SubmitTask
        {
            public DateTime CreateTime;
            public SubmitStatus Status;
            public string SavePath;
            public SubmitData Data;
        };

        public List<SubmitTask> SubmitTasks { get; private set; }

        private IExercise service;

        public SubmitModel()
        {
            SubmitTasks = new List<SubmitTask>();
            service = Services.Get<IExercise>();
        }

        public SubmitTask AddTask()
        {
            string path = System.Environment.CurrentDirectory
                + "\\扫描试卷\\" + DateTime.Now.ToString("D") + "\\" + DateTime.Now.ToString("T").Replace(':', '.');
            Directory.CreateDirectory(path);
            SubmitTask task = new SubmitTask() { CreateTime = DateTime.Now, SavePath = path, Status = SubmitStatus.Scaning };
            SubmitTasks.Insert(0, task);
            return task;
        }

        public async void Submit(SubmitTask task)
        {
            task.Status = SubmitStatus.Submitting;
            await service.Submit(task.Data);
            List<string> pageNames = task.Data.data.SelectMany(s => s.pages.Select(p => p.imageName)).ToList();
            Dictionary<string, string> pageUrls = await service.GeneratePresignedUrls(new GenUriData() { ObjectNameList = pageNames });
            HttpClient hc = new HttpClient();
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "image/jpg");
            foreach (var p in pageUrls)
            {
                FileStream fs = new FileStream(task.SavePath + "\\" + p.Key, FileMode.Open, FileAccess.Read);
                StreamContent content = new StreamContent(fs);
                content.Headers.Add("Content-Type", "image/jpg");
                var response = await hc.PutAsync(p.Value, content);
                if (response.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                    throw new HttpResponseException(response.StatusCode, response.ReasonPhrase);
            }
        }

    }

}
