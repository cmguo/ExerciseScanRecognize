using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TalBase.View
{
    public partial class TalToggle : ToggleButton
    {
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

        private TextBlock text1;
        private TextBlock text2;

        public TalToggle()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //var border = VisualTreeHelper.GetChild(this, 0);
            //var content = VisualTreeHelper.GetChild(border, 0);
            //var border2 = VisualTreeHelper.GetChild(content, 0);
            //var grid = VisualTreeHelper.GetChild(border2, 0);
            //text1 = VisualTreeHelper.GetChild(grid, 0) as TextBlock;
            //text2 = VisualTreeHelper.GetChild(grid, 1) as TextBlock;
            //text1.MouseLeftButtonDown += Text_MouseLeftButtonDown;
            //text2.MouseLeftButtonDown += Text_MouseLeftButtonDown;
        }

        private void Text_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsChecked = sender == text2;
        }

    }
}
