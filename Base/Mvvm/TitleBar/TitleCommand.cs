using System.Windows;
using System.Windows.Input;

namespace Base.TitleBar
{
    public class TitleCommand : Freezable
    {
        #region Dependency Properties

        public static DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
                                        typeof(string),
                                        typeof(TitleCommand));
        public static DependencyProperty CommandProperty =
            DependencyProperty.Register("Command",
                                        typeof(ICommand),
                                        typeof(TitleCommand));
        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter",
                                        typeof(ICommand),
                                        typeof(object));

        #endregion // Dependency Properties

        #region Constructor

        public TitleCommand()
        {
        }

        #endregion // Constructor

        #region Properties

        public string Name
        {
            get { return GetValue(NameProperty) as string; }
            set { SetValue(NameProperty, value); }
        }
        public ICommand Command
        {
            get { return GetValue(CommandProperty) as ICommand; }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion // Properties

        #region Public Methods

        #endregion // Public Methods

        #region Private Methods

        #endregion // Private Methods

        #region Freezable overrides

        protected override void CloneCore(Freezable sourceFreezable)
        {
            TitleCommand pushBinding = sourceFreezable as TitleCommand;
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TitleCommand();
        }

        #endregion // Freezable overrides
    }
}
