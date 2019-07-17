using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Base.Mvvm
{
    /// <summary>
    /// This RelayCommand is taken from MSDN magazine
    /// http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
    /// </summary>

    public class RelayCommand : ICommand, INotifyPropertyChanged
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

        public static event EventHandler<ActionExceptionEventArgs> ActionException;

        public enum ExecuteStatus
        {
            Free, 
            Busy, 
            Error
        }

        public delegate Task AsyncAction<in T>(T obj);

        public event EventHandler<ActionExceptionEventArgs> ExceptionRaised;

        public event PropertyChangedEventHandler PropertyChanged;

        public ExecuteStatus Status { get; private set; }

        public Exception Exception { get; private set; }

        readonly Action<object> _execute;
        readonly AsyncAction<object> _asyncExecute;
        readonly Predicate<object> _canExecute;
        private bool executing;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
            Status = ExecuteStatus.Free;
        }

        public RelayCommand(AsyncAction<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(AsyncAction<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _asyncExecute = execute;
            _canExecute = canExecute;
            Status = ExecuteStatus.Free;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return !executing && (_canExecute == null ? true : _canExecute(parameter));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public async void Execute(object parameter)
        {
            try
            {
                Status = ExecuteStatus.Busy;
                RaisePropertyChanged("Status");
                if (_execute != null)
                {
                    _execute(parameter);
                }
                else
                {
                    executing = true;
                    CommandManager.InvalidateRequerySuggested();
                    await _asyncExecute(parameter);
                }
                Status = ExecuteStatus.Free;
                RaisePropertyChanged("Status");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Status = ExecuteStatus.Error;
                RaisePropertyChanged("Status");
                Exception = e;
                if (RaisePropertyChanged("Exception"))
                    return;
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e) { Parameter = parameter };
                ExceptionRaised?.Invoke(this, e1);
                if (!e1.IsHandled)
                    ActionException?.Invoke(this, e1);
                if (!e1.IsHandled)
                    throw e;
            }
            finally
            {
                executing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        protected bool RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion // ICommand Members
    }

}