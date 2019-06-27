using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;

namespace Base.TitleBar
{
    public class TitleButton : Freezable
    {
        #region Dependency Properties

        public static DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
                                        typeof(object),
                                        typeof(TitleButton));
        public static DependencyProperty ContentProperty =
            DependencyProperty.Register("Content",
                                        typeof(object),
                                        typeof(TitleButton));

        public Dock? GetDock()
        {
            return (Dock) GetValue(DockPanel.DockProperty);
        }

        #endregion // Dependency Properties

        #region Constructor

        public TitleButton()
        {
        }

        #endregion // Constructor

        #region Properties

        public object Name
        {
            get { return GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        [DefaultValue(null)]
        public string TargetProperty
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public DependencyProperty TargetDependencyProperty
        {
            get;
            set;
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
            TargetProperty = pushBinding.TargetProperty;
            TargetDependencyProperty = pushBinding.TargetDependencyProperty;
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TitleButton();
        }

        #endregion // Freezable overrides
    }
}
