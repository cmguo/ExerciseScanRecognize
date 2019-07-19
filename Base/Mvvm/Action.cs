using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Base.Mvvm
{

    public class Action : NotifyBase
    {
        #region Fields

        public class ActionExceptionEventArgs : EventArgs
        {
            public ActionExceptionEventArgs(Exception e)
            {
                Exception = e;
                IsHandled = false;
            }

            public Object Parameter { get; internal set; }
            public Exception Exception { get; internal set; }
            public bool IsHandled { get; set; }
        }

        internal static void RaiseException(object owner, ActionExceptionEventArgs e1)
        {
            ActionException?.Invoke(owner, e1);
        }

        public static event EventHandler<ActionExceptionEventArgs> ActionException;

        public enum ActionStatus
        {
            Free, 
            Busy, 
            Error
        }

        public delegate Task AsyncAction<in T>(T obj);

        public event EventHandler<ActionExceptionEventArgs> ExceptionRaised;

        public ActionStatus Status { get; private set; }

        public Exception Exception { get; private set; }

        readonly Action<object> _action;
        readonly AsyncAction<object> _asyncAction;

        #endregion // Fields

        #region Constructors

        public Action(Action<object> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
            Status = ActionStatus.Free;
        }

        public Action(AsyncAction<object> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _asyncAction = action;
            Status = ActionStatus.Free;
        }

        #endregion // Constructors

        #region ICommand Members

        public virtual async void Execute(object parameter)
        {
            try
            {
                Status = ActionStatus.Busy;
                RaisePropertyChanged("Status");
                if (_action != null)
                {
                    _action(parameter);
                }
                else
                {
                    CommandManager.InvalidateRequerySuggested();
                    await _asyncAction(parameter);
                }
                Status = ActionStatus.Free;
                RaisePropertyChanged("Status");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Status = ActionStatus.Error;
                RaisePropertyChanged("Status");
                Exception = e;
                if (RaisePropertyChanged("Exception"))
                    return;
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e) { Parameter = parameter };
                ExceptionRaised?.Invoke(this, e1);
                if (!e1.IsHandled)
                    ActionException?.Invoke(this, e1);
                if (!e1.IsHandled)
                    throw;
            }
        }

        #endregion // ICommand Members
    }

}