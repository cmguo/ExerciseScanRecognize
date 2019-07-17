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

    public class RelayCommand : Action, ICommand
    {
        #region Fields

        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            : base(execute)
        {
            _canExecute = canExecute;
        }

        public RelayCommand(AsyncAction<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(AsyncAction<object> execute, Predicate<object> canExecute)
            : base(execute)
        {
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return Status != ActionStatus.Busy && (_canExecute == null ? true : _canExecute(parameter));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public override void Execute(object parameter)
        {
            try
            {
                base.Execute(parameter);
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion // ICommand Members
    }

}