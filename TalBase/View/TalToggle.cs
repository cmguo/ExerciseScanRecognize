using System.Windows;
using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalToggle : ToggleButton
    {
        public enum ToggleStyles
        {
            Normal,
            Solid,
            Switch,
        }

        public static DependencyProperty ToggleStyleProperty =
            DependencyProperty.Register("ToggleStyle", typeof(ToggleStyles), typeof(TalToggle), new PropertyMetadata(ToggleStyles.Normal));

        public ToggleStyles ToggleStyle
        {
            get { return (ToggleStyles)GetValue(ToggleStyleProperty); }
            set { SetValue(ToggleStyleProperty, value); }
        }

        public static DependencyProperty Title1Property =
            DependencyProperty.Register("Title1", typeof(string), typeof(TalToggle));

        public static DependencyProperty Title2Property =
            DependencyProperty.Register("Title2", typeof(string), typeof(TalToggle));

        public string Title1
        {
            get { return GetValue(Title1Property) as string; }
            set
            {
                SetValue(Title1Property, value);
            }
        }

        public string Title2
        {
            get { return GetValue(Title2Property) as string; }
            set
            {
                SetValue(Title2Property, value);
            }
        }

        static TalToggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalToggle), new FrameworkPropertyMetadata(typeof(TalToggle)));
        }

    }
}
