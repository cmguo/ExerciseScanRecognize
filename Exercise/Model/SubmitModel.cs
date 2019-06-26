using Base.Misc;
using Base.Service;
using Exercise.Algorithm;
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

        public enum TaskStatus
        {
            Wait, 
            Submiting, 
            Completed, 
            Failed
        }

        public class SubmitTask : ModelBase
        {
            public string Path { get; internal set; }
            public int Total { get; internal set; }
            private int _Finish;
            public int Finish
            {
                get { return _Finish; }
                internal set { _Finish = value; RaisePropertyChanged("Finish"); }
            }
            private TaskStatus _Status;
            public TaskStatus Status
            {
                get { return _Status; }
                internal set { _Status = value; RaisePropertyChanged("Status"); }
            }
            public SubmitPrepare Prepare { get; set; }
            public SubmitData Submit { get; set; }
        }

        public Dictionary<string, SubmitTask> SubmitTasks { get; private set; }

        private IExercise service;

        public SubmitModel()
        {
            SubmitTasks = new Dictionary<string, SubmitTask>();
            service = Services.Get<IExercise>();
        }

        public async Task Save(string path, string exerciseId, ICollection<ClassInfo> classes, ICollection<StudentInfo> students)
        {
            IList<SubmitData.AnswerInfo> data = students
                .Where(s => s.AnswerPages != null && s.AnswerPages.Any(p => p != null && p.Answer != null))
                .Select(s => new SubmitData.AnswerInfo() { StudentId = s.Id, PageInfo = GetAnswers(s) })
                .ToList();
            SubmitData sdata = new SubmitData() { PaperId = exerciseId, Data = data };
            SubmitPrepare prepare = new SubmitPrepare() { PaperId = exerciseId, ClassIdList = classes.Select(c => c.ClassId).ToList() };
            SubmitTask task = new SubmitTask() { Path = path, Status = TaskStatus.Wait, Prepare = prepare, Submit = sdata };
            SubmitTasks[path] = task;
            await JsonPersistent.Save(path + "\\submit.json", task);
        }

        public async Task Submit(string path)
        {
            SubmitTask task = SubmitTasks[path];
            if (task == null)
            {
                task = await JsonPersistent.Load<SubmitTask>(path + "\\submit.json");
                SubmitTasks[path] = task;
            }
            await Submit(task);
        }

        public async Task Submit(SubmitTask task)
        {
            try
            {
                task.Status = TaskStatus.Submiting;
                await SubmitInner(task);
            }
            catch (Exception e)
            {
                task.Status = TaskStatus.Failed;
                await JsonPersistent.Save(task.Path + "\\submit.json", task);
                throw e;
            }
        }

        private IList<AnswerData> GetAnswers(StudentInfo s)
        {
            List<AnswerData> answers = new List<AnswerData>();
            foreach (Page p in s.AnswerPages)
            {
                if (p == null)
                    continue;
                if (p.Answer != null)
                {
                    p.Answer.ImageName = p.Md5Name;
                    p.Answer.PageId = p.PageIndex;
                    answers.Add(p.Answer);
                }
                if (p.Another != null && p.Another.Answer != null)
                {
                    p.Another.Answer.ImageName = p.Another.Md5Name;
                    p.Another.Answer.PageId = p.Another.PageIndex;
                    answers.Add(p.Another.Answer);
                }
            }
            return answers;
        }

        private async Task SubmitInner(SubmitTask task)
        {
            SubmitData sdata = task.Submit;
            task.Total = (sdata.Data.Count + SUBIT_BATCH_SIZE - 1) / SUBIT_BATCH_SIZE 
                + sdata.Data.Select(d => d.PageInfo).Count();
            task.Finish = sdata.Data.SelectMany(d => d.PageInfo.Where(p => p.ImageName == null)).Count();
            if (sdata.HomeworkId == null)
            {
                StringData data = await service.GetSubmitId(task.Prepare);
                sdata.HomeworkId = data.Value;
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
            ICollection<AnswerData> pages = sdata.Data.SelectMany(s => s.PageInfo.Where(p => p.ImageName != null)).ToList();
            List<string> pageNames = pages.Select(p => p.ImageName).ToList();
            Dictionary<string, string> pageUrls = await service.GeneratePresignedUrls(new GenUriData() { ObjectNameList = pageNames });
            HttpClient hc = new HttpClient();
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "image/jpg");
            foreach (AnswerData p in pages)
            {
                FileStream fs = new FileStream(task.Path + "\\" + p.ImageName, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "image/jpg");
                    var response = await hc.PutAsync(pageUrls[p.ImageName], content);
                    if (response.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                        throw new HttpResponseException(response.StatusCode, response.ReasonPhrase);
                }
                fs.Close();
                p.ImageName = null;
                ++task.Finish;
            }
            SubmitTasks.Remove(task.Path);
            Directory.Delete(task.Path, true);
            task.Status = TaskStatus.Completed;
        }

    }

}
