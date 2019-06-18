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
using TalBase.Model;

namespace Exercise.Model
{
    class SubmitModel : ModelBase
    {
        private const int SUBIT_BATCH_SIZE = 50;

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

        public class SubmitTask : ModelBase
        {
            public int Total { get; set; }
            private int _Finish;
            public int Finish
            {
                get { return _Finish;  }
                set { _Finish = value; RaisePropertyChanged("Finish"); }
            }
            public SubmitData Data { get; set; }
        }

        public Dictionary<string, SubmitTask> SubmitTasks { get; private set; }

        private IExercise service;

        public SubmitModel()
        {
            SubmitTasks = new Dictionary<string, SubmitTask>();
            service = Services.Get<IExercise>();
        }

        public async Task Save(string path, ExerciseData exercise, ICollection<StudentInfo> students)
        {
            IList<SubmitData.AnswerInfo> data = students.Select(s => new SubmitData.AnswerInfo()
            {
                StudentId = s.StudentNo,
                PageInfo = s.AnswerPages.Where(p => p != null).Select(p => p.Answer).ToList()
            }).ToList();
            SubmitData sdata = new SubmitData() { HomeworkId = exercise.ExerciseId, PaperId = exercise.ExerciseId, Data = data };
            SubmitTasks[path] = new SubmitTask() { Data = sdata };
            await JsonPersistent.Save(path + "\\submit.json", sdata);
        }

        public async Task Submit(string path)
        {
            SubmitTask task = SubmitTasks[path];
            if (task == null)
            {
                SubmitData sdata1 = await JsonPersistent.Load<SubmitData>(path + "\\submit.json");
                SubmitTasks[path] = new SubmitTask() { Data = sdata1 };
            }
            SubmitData sdata = task.Data;
            task.Total = (sdata.Data.Count + SUBIT_BATCH_SIZE - 1) / SUBIT_BATCH_SIZE + sdata.Data.Select(d => d.PageInfo).Count();
            task.Finish = 0;
            if (sdata.HomeworkId == null)
            {
                sdata.HomeworkId = await service.GetSubmitId();
            }
            IList<SubmitData.AnswerInfo> list = sdata.Data;
            int i = 0;
            for (; i + SUBIT_BATCH_SIZE < list.Count; i += SUBIT_BATCH_SIZE)
            {
                sdata.Data = list.Skip(0).Take(SUBIT_BATCH_SIZE).ToList();
                await service.Submit(sdata);
                ++task.Finish;
            }
            if (i > 0)
                sdata.Data = list.Skip(i).ToList();
            await service.Submit(sdata);
            ++task.Finish;
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
                ++task.Finish;
            }
            SubmitTasks.Remove(path);
            Directory.Delete(path, true);
        }

    }

}
