using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Base.Mvvm.RelayCommand;

namespace Base.Mvvm
{
    public class BackgroudWork
    {
        public delegate Task Work();

        public static event EventHandler<ActionExceptionEventArgs> WorkException;

        public static async void Execute(Work work, object owner)
        {
            try
            {
                await work();
            }
            catch (Exception e)
            {
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e);
                WorkException?.Invoke(owner, e1);
                if (!e1.IsHandled)
                    throw e;
            }
        }

        public static void Execute(Work work)
        {
            Execute(work, work);
        }
    }
}
