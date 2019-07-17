using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Base.Mvvm.RelayCommand;

namespace Base.Mvvm
{
    public class BackgroudWork
    {
        public delegate Task Work();

        public delegate bool OnError(Exception e);

        public static event EventHandler<ActionExceptionEventArgs> WorkException;

        public static async void Execute(Work work, OnError onError, object owner)
        {
            try
            {
                await work();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (onError != null && onError(e))
                    return;
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e);
                WorkException?.Invoke(owner, e1);
                if (!e1.IsHandled)
                    throw e;
            }
        }
        public static void Execute(Work work, object owner)
        {
            Execute(work, null, owner);
        }

        public static void Execute(Work work, OnError onError)
        {
            Execute(work, onError, work);
        }

        public static void Execute(Work work)
        {
            Execute(work, work);
        }
    }
}
