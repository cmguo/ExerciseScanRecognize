using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Base.Mvvm.Action;

namespace Base.Mvvm
{
    public class BackgroudWork
    {
        public delegate Task Work();

        public static async void Execute(Work work, object owner)
        {
            try
            {
                await work();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e);
                Action.RaiseException(owner, e1);
                if (!e1.IsHandled)
                    throw;
            }
        }

        public static async void Execute(Task task, object owner)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e);
                Action.RaiseException(owner, e1);
                if (!e1.IsHandled)
                    throw;
            }
        }

        public static void Execute(Work work)
        {
            Execute(work, work);
        }

        public static void Execute(Task task)
        {
            Execute(task, task);
        }
    }
}
