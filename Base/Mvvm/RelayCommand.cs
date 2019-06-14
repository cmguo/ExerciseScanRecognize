﻿using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Base.Mvvm
{
    /// <summary>
    /// This RelayCommand is taken from MSDN magazine
    /// http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
    /// </summary>

    public class RelayCommand : ICommand
    {
        #region Fields

        public class ActionExceptionEventArgs : EventArgs
        {
            public ActionExceptionEventArgs(Exception e)
            {
                Exception = e;
                IsHandled = false;
            }

            public Exception Exception { get; private set; }
            public bool IsHandled { get; set; }
        }

        public static event EventHandler<ActionExceptionEventArgs> ActionException;

        public delegate Task AsyncAction<in T>(T obj);

        readonly Action<object> _execute;
        readonly AsyncAction<object> _asyncExecute;
        readonly Predicate<object> _canExecute;

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
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
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
                if (_execute != null)
                    _execute(parameter);
                else
                    await _asyncExecute(parameter);
            }
            catch (Exception e)
            {
                ActionExceptionEventArgs e1 = new ActionExceptionEventArgs(e);
                ActionException?.Invoke(this, e1);
                if (!e1.IsHandled)
                    throw e;
            }
        }

        #endregion // ICommand Members
    }

}