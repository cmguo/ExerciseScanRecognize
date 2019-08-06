using Base.Misc;
using Base.Service;
using Exercise.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TalBase.Model;

namespace Exercise.Model
{
    class SubmitModel : ModelBase
    {
        private static readonly Logger Log = Logger.GetLogger<SubmitModel>();

        private const int SUBIT_BATCH_SIZE = 10;

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
            //public string Path { get; internal set; }
            public int Total { get; set; }
            private int _Finish;
            public int Finish
            {
                get { return _Finish; }
                set { _Finish = value; RaisePropertyChanged("Finish"); }
            }

            [JsonIgnore]
            public int Left => (Submit.HomeworkId == null ? 1 : 0) 
                + (Submit.Data.Count + SUBIT_BATCH_SIZE - 1) / SUBIT_BATCH_SIZE + 1 
                + PageNames.Count + 1;

            private TaskStatus _Status;
            public TaskStatus Status
            {
                get { return _Status; }
                set {
                    _Status = value;
                    RaisePropertyChanged("Status");
                    lock (this)
                    {
                        Monitor.PulseAll(this);
                    }
                }
            }
            public SubmitPrepare Prepare { get; set; }
            public SubmitData Submit { get; set; }
            public IList<string> PageNames { get; set; }

            internal string path;
            internal bool cancel;

            internal async Task Save()
            {
                await JsonPersistent.SaveAsync(path + "\\submit.json", this);
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
            submitAction.ExceptionRaised += (s, e) => { e.IsHandled = true; }; // avoid error window
        }

        public async Task Save(string path, string exerciseId, ICollection<ClassInfo> classes, ICollection<StudentInfo> students)
        {
            IList<SubmitData.AnswerInfo> data = students
                .Where(s => s.AnswerPages != null && s.AnswerPages.Any(p => p != null && p.Answer != null))
                .Select(s => new SubmitData.AnswerInfo() { StudentId = s.Id, PageInfo = s.Answers })
                .ToList();
            SubmitData sdata = new SubmitData() { PaperId = exerciseId, Data = data };
            SubmitPrepare prepare = new SubmitPrepare() { PaperId = exerciseId, ClassIdList = classes.Select(c => c.ClassId).ToList() };
            IList<string> names = data.SelectMany(s => s.PageInfo.Select(p => p.ImageName)).ToList();
            SubmitTask task = new SubmitTask() { Status = TaskStatus.Wait, Prepare = prepare, Submit = sdata, PageNames = names };
            task.path = path;
            task.Total = task.Left;
            SubmitTasks[path] = task;
            await task.Save();
        }

        public async Task<SubmitTask> Load(string path)
        {
            SubmitTask task = null;
            if (!SubmitTasks.TryGetValue(path, out task))
            {
                if (File.Exists(path + "\\submit.json"))
                {
                    try
                    {
                        task = await JsonPersistent.LoadAsync<SubmitTask>(path + "\\submit.json");
                        task.path = path;
                        SubmitTasks[path] = task;
                    }
                    catch (Exception e)
                    {
                        Log.w("Load", e);
                        File.Delete(path + "\\submit.json");
                    }
                }
            }
            return task;
        }

        public async Task Submit(string path)
        {
            SubmitTask task = await Load(path);
            submitAction.Execute(task);
        }

        public Task Cancel(SubmitTask task)
        {
            task.cancel = true;
            return Task.Run(() =>
            {
                lock (task)
                {
                    while (task.Status == TaskStatus.Submiting)
                    {
                        Monitor.Wait(task);
                    }
                }
            });
        }

        public void Submit(SubmitTask task)
        {
            submitAction.Execute(task);
        }

        private async Task SubmitWork(SubmitTask task)
        {
            try
            {
                HistoryModel.Instance.BeginDuration(HistoryModel.DurationType.Submit);
                task.Status = TaskStatus.Submiting;
                await SubmitInner(task);
            }
            catch
            {
                task.Status = TaskStatus.Failed;
                throw;
            }
            finally
            {
                HistoryModel.Instance.EndDuration();
                if (task.Status != TaskStatus.Completed)
                    await task.Save();
                else
                    HistoryModel.Instance.Clear();
            }
        }

        private async Task SubmitInner(SubmitTask task)
        {
            SubmitData sdata = task.Submit;
            task.Finish = task.Total - task.Left;
            if (sdata.HomeworkId == null)
            {
                StringData data = await service.GetSubmitId(task.Prepare);
                sdata.HomeworkId = data.Value;
                await task.Save();
                ++task.Finish;
            }
            await SubmitInfo(task, sdata);
            if (task.cancel)
                return;
            await SubmitImages(task);
            if (task.cancel)
                return;
            await service.CompleteSubmit(new SubmitComplete() { HomeworkId = sdata.HomeworkId });
            ++task.Finish;
            SubmitTasks.Remove(task.path);
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
                    if (task.cancel)
                        break;
                    sdata.Data = list.Skip(i).Take(SUBIT_BATCH_SIZE).ToList();
                    await service.Submit(sdata);
                    ++task.Finish;
                    if (task.Finish % 5 == 0)
                        await task.Save();
                }
                if (task.cancel)
                    return;
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
                if (task.cancel)
                    break;
                string n = pageNames.First();
                using (FileStream fs = new FileStream(task.path + "\\" + n, FileMode.Open, FileAccess.Read))
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
