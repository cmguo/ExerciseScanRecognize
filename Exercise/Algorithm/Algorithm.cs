using Base.Misc;
using Base.Mvvm;
using com.talcloud.paperanalyze.service.answersheet;
using net.sf.jni4net;
using net.sf.jni4net.jni;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class Algorithm
    {
        private static readonly Logger Log = Logger.GetLogger<Algorithm>();

        private static readonly Encoding ServiceEncoidng = Encoding.GetEncoding("gbk");

        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        private Thread[] threadPool = new Thread[4];
        private Queue<Task> tasks = new Queue<Task>();

        private ProcessStartInfo processInfo;
        private Process process;
        private Thread readThread;
        private int taskId = 0;
        private Dictionary<int, RemoteTask> remoteTasks;

        public Algorithm(bool? client = null)
        {
            if (client == null || client == false)
            {
                if (client == null)
                {
                    BackgroudWork.Execute(() => Task.Run(() =>
                    {
                        Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
                        AnswerSheetAnalyze.init();
                        //Test();
                    }));
                }
                else
                {
                    Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
                    AnswerSheetAnalyze.init();
                }
                for (int i = 0; i < threadPool.Length; ++i)
                {
                    threadPool[i] = new Thread(AlgorithmThread);
                    threadPool[i].Name = "Algorithm " + i;
                    threadPool[i].IsBackground = true;
                    threadPool[i].Priority = ThreadPriority.AboveNormal;
                    threadPool[i].Start();
                }
            }
            else if (client == true)
            {
                Bridge.RegisterAssembly(typeof(AnswerSheetAnalyze).Assembly);
                readThread = new Thread(ReadService);
                readThread.IsBackground = true;
                readThread.Name = "AlgorithmRead";
                processInfo = new ProcessStartInfo()
                {
                    FileName = "Service.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = ServiceEncoidng
                };
                remoteTasks = new Dictionary<int, RemoteTask>();
                readThread.Start();
            }
        }

        public void Shutdown()
        {
            try
            {
                lock (remoteTasks)
                {
                    if (process != null)
                        process.StandardInput.Close();
                }
            }
            catch
            {
            }
        }

        public async Task<QRCodeData> GetCode(PageRaw page)
        {
            QRCodeData code = await Analyze<QRCodeData, PageRaw>(AnswerSheetAnalyze.METHOD_QR_CODE_RECOGNIZE, page);
            if (code.PaperInfo == "")
                code.PaperInfo = null;
            if (code.StudentInfo == "")
                code.StudentInfo = null;
            return code;
        }

        public Task<AnswerData> GetAnswer(PageData page)
        {
            return Analyze<AnswerData, PageData>(AnswerSheetAnalyze.METHOD_ANSWER_SHEET_ANALYZE, page);
        }

        private int lastLength = 0;

        private async Task<O> Analyze<O, I>(string method, I input)
        {
            try
            {
                string args = JsonConvert.SerializeObject(input, settings);
                string result = await Analyze(method, args);
                if (result.Length > lastLength)
                {
                    lastLength = result.Length;
                    Log.d("Analyze " + lastLength);
                }
                Result<O> output = JsonConvert.DeserializeObject<Result<O>>(result, settings);
                if (output.Code != 0)
                    throw new AlgorithmException(output.Code, output.Message);
                return output.Data;
            }
            catch (AlgorithmException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.w(e);
                throw new Exception("识别异常", e);
            }
        }

        public Task<string> Analyze(string method, string body)
        {
            if (remoteTasks == null)
            {
                Task<string> task = new Task<string>(
                    () => AnswerSheetAnalyze.analyzeAnswerSheet(method, body));
                lock (tasks)
                {
                    tasks.Enqueue(task);
                    Monitor.PulseAll(tasks);
                }
                return task;
            }
            else
            {
                int id;
                RemoteTask task = new RemoteTask(method, body);
                lock (remoteTasks)
                {
                    id = ++taskId;
                    remoteTasks.Add(id, task);
                    if (process != null)
                    {
                        Process p = process;
                        Task.Run(() =>
                        {
                            task.Write(p.StandardInput, id);
                        });
                    }
                }
                return task.tcs.Task;
            }
        }

        private class RemoteTask
        {
            internal string method;
            internal string body;
            internal TaskCompletionSource<string> tcs = 
                new TaskCompletionSource<string>();

            internal RemoteTask(string m, string b)
            {
                method = m;
                body = b;
            }

            internal void Write(StreamWriter writer, int id)
            {
                Log.d("Analyze send " + id);
                lock (writer)
                {
                    writer.WriteLine(id.ToString());
                    writer.WriteLine(method);
                    writer.WriteLine(body);
                }
            }

            internal void Read(StreamReader reader, int id)
            {
                Log.d("Analyze recv " + id);
                try
                {
                    string result = reader.ReadLine();
                    if (result == null)
                        throw new Exception("算法服务异常，服务断开");
                    if (result.StartsWith("{") && result.EndsWith("}"))
                        tcs.SetResult(result.Replace("\\n", "\n").Replace("\\r", "\r"));
                    else
                        tcs.SetException(new Exception(result.Replace("\\n", "\n").Replace("\\r", "\r")));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
        }

        private void AlgorithmThread()
        {
            JNIEnv env = JNIEnv.ThreadEnv;
            while (true)
            {
                Task task = null;
                lock (tasks)
                {
                    while (tasks.Count == 0)
                        Monitor.Wait(tasks);
                    task = tasks.Dequeue();
                }
                if (task == null)
                {
                    lock (tasks)
                    {
                        tasks.Enqueue(null);
                    }
                    break;
                }
                task.RunSynchronously();
            }
            JNIEnv.DetachCurrentThread();
        }

        public void ServiceMain(string[] args)
        {
            StreamReader reader = new StreamReader(Console.OpenStandardInput(), ServiceEncoidng);
            StreamWriter writer = new StreamWriter(Console.OpenStandardOutput(), ServiceEncoidng);
            int pid = Process.GetCurrentProcess().Id;
            writer.WriteLine(pid);
            writer.Flush();
            Log.d("ServiceMain enter " + pid);
            while (true)
            {
                string id = reader.ReadLine();
                if (id == null || id.Length > 20)
                    break;
                string method = reader.ReadLine();
                string body = reader.ReadLine();
                Analyze(writer, id, method, body);
            }
            Log.d("ServiceMain exit " + pid);
        }

        private async void Analyze(StreamWriter writer, string id, string method, string body)
        {
            try
            {
                Log.d("Analyze in " + id);
                string result = await Analyze(method, body);
                Log.d("Analyze out " + id);
                lock (writer)
                {
                    writer.WriteLine(id);
                    writer.WriteLine(result.Replace("\r", "\\r").Replace("\n", "\\n"));
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                lock (writer)
                {
                    writer.WriteLine(id);
                    writer.WriteLine(e.ToString().Replace("\r", "\\r").Replace("\n", "\\n"));
                    writer.Flush();
                }
            }
        }

        private void ReadService()
        {
            while (readThread != null)
            {
                if (process == null)
                {
                    Process p = new Process() { StartInfo = processInfo };
                    p.Start();
                    Log.d("ReadService encoding=" + p.StandardInput.Encoding);
                    string pid = p.StandardOutput.ReadLine();
                    Log.d("ReadService pid=" + pid);
                    lock (remoteTasks)
                    {
                        foreach (var t in remoteTasks)
                        {
                            Task.Run(() =>
                            {
                                t.Value.Write(p.StandardInput, t.Key);
                            });
                        }
                        process = p;
                    }
                }
                try
                {
                    string idstr = process.StandardOutput.ReadLine();
                    while (idstr.Contains("CodeRecognize"))
                        idstr = process.StandardOutput.ReadLine();
                    Log.d("ReadService idstr=" + idstr);
                    int id = Int32.Parse(idstr);
                    RemoteTask task = null;
                    lock (remoteTasks)
                    {
                        if (!remoteTasks.TryGetValue(id, out task))
                            throw new Exception("算法服务异常，请求ID不匹配");
                        remoteTasks.Remove(id);
                    }
                    task.Read(process.StandardOutput, id);
                }
                catch (Exception e)
                {
                    Log.w("ReadService", e);
                    if (!process.HasExited)
                    {
                        process.StandardOutput.Close();
                    }
                    lock (remoteTasks)
                    {
                        process = null;
                    }
                }
            }
        }

        public async void Test()
        {
            string json = @"C:\Users\Brandon\source\repos\DotNet\Exercise\0620\long_text_2019-06-20-16-53-51.txt";
            string jpg = @"C:\Users\Brandon\source\repos\DotNet\Exercise\0620\3.jpg";
            PageRaw pagew = new PageRaw();
            pagew.ImgPathIn = jpg;
            QRCodeData code = await GetCode(pagew);
            PageData page = await JsonPersistent.Load<PageData>(json);
            page.ImgPathIn = jpg;
            AnswerData answer = await GetAnswer(page);
        }

    }
}
