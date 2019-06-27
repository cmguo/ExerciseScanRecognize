using System.Windows;
using System.Windows.Controls;

namespace Base.TitleBar
{
    public class TitleButton : Freezable
    {
        #region Dependency Properties

        public static DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
                                        typeof(string),
                                        typeof(TitleButton));
        public static DependencyProperty ContentProperty =
            DependencyProperty.Register("Content",
                                        typeof(FrameworkElement),
                                        typeof(TitleButton));

        public Dock? GetDock()
        {
            return (Dock) Content.GetValue(DockPanel.DockProperty);
        }

        #endregion // Dependency Properties

        #region Constructor

        public TitleButton()
        {
        }

        #endregion // Constructor

        #region Properties

        public string Name
        {
            get { return GetValue(NameProperty) as string; }
            set { SetValue(NameProperty, value); }
        }
        public FrameworkElement Content
        {
            get { return GetValue(ContentProperty) as FrameworkElement; }
            set { SetValue(ContentProperty, value); }
        }

        #endregion // Properties

        #region Public Methods

        #endregion // Public Methods

        #region Private Methods

        #endregion // Private Methods

        #region Freezable overrides

        protected override void CloneCore(Freezable sourceFreezable)
        {
            TitleButton pushBinding = sourceFreezable as TitleButton;
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TitleButton();
        }

        #endregion // Freezable overrides
    }
}
