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
            Failed, 
            Cancel, 
            Canceled
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
            public IList<string> PageNames { get; set; }

            internal async Task Save()
            {
                await JsonPersistent.Save(Path + "\\submit.json", this);
            }
        }

        public Dictionary<string, SubmitTask> SubmitTasks { get; private set; }

        private IExercise service;

        private Base.Mvvm.Action submitAction;

        public SubmitModel()
        {
            SubmitTasks = new Dictionary<string, SubmitTask>();
            service = Services.Get<IExercise>();
            submitAction = new Base.Mvvm.Action((e) => SubmitWork(e as SubmitTask));
            submitAction.ExceptionRaised += (s, e) => { }; // avoid error window
        }

        public async Task Save(string path, string exerciseId, ICollection<ClassInfo> classes, ICollection<StudentInfo> students)
        {
            IList<SubmitData.AnswerInfo> data = students
                .Where(s => s.AnswerPages != null && s.AnswerPages.Any(p => p != null && p.Answer != null))
                .Select(s => new SubmitData.AnswerInfo() { StudentId = s.Id, PageInfo = GetAnswers(s) })
                .ToList();
            SubmitData sdata = new SubmitData() { PaperId = exerciseId, Data = data };
            SubmitPrepare prepare = new SubmitPrepare() { PaperId = exerciseId, ClassIdList = classes.Select(c => c.ClassId).ToList() };
            IList<string> names = data.SelectMany(s => s.PageInfo.Select(p => p.ImageName)).ToList();
            int total = (data.Count + SUBIT_BATCH_SIZE - 1) / SUBIT_BATCH_SIZE + 1 + names.Count + 1;
            SubmitTask task = new SubmitTask() { Path = path, Status = TaskStatus.Wait, Total = total, Prepare = prepare, Submit = sdata, PageNames = names };
            SubmitTasks[path] = task;
            await task.Save();
        }

        public async Task Submit(string path)
        {
            SubmitTask task = SubmitTasks[path];
            if (task == null)
            {
                task = await JsonPersistent.Load<SubmitTask>(path + "\\submit.json");
                SubmitTasks[path] = task;
            }
            submitAction.Execute(task);
        }

        public async Task Cancel(SubmitTask task)
        {
            task.Status = TaskStatus.Cancel;
            await task.Save();
        }

        public void Submit(SubmitTask task)
        {
            submitAction.Execute(task);
        }

        private async Task SubmitWork(SubmitTask task)
        {
            try
            {
                task.Status = TaskStatus.Submiting;
                await SubmitInner(task);
            }
            catch (Exception e)
            {
                task.Status = TaskStatus.Failed;
                await task.Save();
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
                    p.Answer.ImageName = p.PageName;
                    p.Answer.PageId = p.PageIndex;
                    answers.Add(p.Answer);
                }
                if (p.Another != null && p.Another.Answer != null)
                {
                    p.Another.Answer.ImageName = p.Another.PageName;
                    p.Another.Answer.PageId = p.Another.PageIndex;
                    answers.Add(p.Another.Answer);
                }
            }
            return answers;
        }

        private async Task SubmitInner(SubmitTask task)
        {
            SubmitData sdata = task.Submit;
            int left = sdata.Data.Count + task.PageNames.Count;
            task.Finish = task.Total - left;
            if (sdata.HomeworkId == null)
            {
                StringData data = await service.GetSubmitId(task.Prepare);
                sdata.HomeworkId = data.Value;
                await JsonPersistent.Save(task.Path + "\\submit.json", task);
            }
            await SubmitInfo(task, sdata);
            await SubmitImages(task);
            await service.CompleteSubmit(new SubmitComplete() { HomeworkId = sdata.HomeworkId });
            ++task.Finish;
            SubmitTasks.Remove(task.Path);
            Directory.Delete(task.Path, true);
            task.Status = TaskStatus.Completed;
        }

        private async Task SubmitInfo(SubmitTask task, SubmitData sdata)
        {
            IList<SubmitData.AnswerInfo> list = sdata.Data;
            if (list.Count == 0)
                return;
            int i = 0;
            try
            {
                for (; i + SUBIT_BATCH_SIZE < list.Count; i += SUBIT_BATCH_SIZE)
                {
                    sdata.Data = list.Skip(i).Take(SUBIT_BATCH_SIZE).ToList();
                    await service.Submit(sdata);
                    ++task.Finish;
                    if (task.Finish % 5 == 0)
                        await task.Save();
                }
                if (i > 0)
                    sdata.Data = list.Skip(i).ToList();
                sdata.Finished = true;
                await service.Submit(sdata);
                i = list.Count;
                ++task.Finish;
                sdata.Data.Clear();
            }
            finally
            {
                if (i < list.Count)
                    sdata.Data = list.Skip(i).ToList();
            }
        }

        private async Task SubmitImages(SubmitTask task)
        {
            IList<string> pageNames = task.PageNames;
            if (pageNames.Count == 0)
                return;
            Dictionary<string, string> pageUrls = await service.GeneratePresignedUrls(new GenUriData() { ObjectNameList = pageNames });
            HttpClient hc = new HttpClient();
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "image/jpg");
            while (pageNames.Count > 0)
            {
                string n = pageNames.First();
                using (FileStream fs = new FileStream(task.Path + "\\" + n, FileMode.Open, FileAccess.Read))
                {
                    StreamContent content = new StreamContent(fs);
                    content.Headers.Add("Content-Type", "image/jpg");
                    var response = await hc.PutAsync(pageUrls[n], content);
                    if (response.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                        throw new HttpResponseException(response.StatusCode, response.ReasonPhrase);
                }
                pageNames.RemoveAt(0);
                ++task.Finish;
                if (task.Finish % 5 == 0)
                    await task.Save();
            }
        }

    }

}
