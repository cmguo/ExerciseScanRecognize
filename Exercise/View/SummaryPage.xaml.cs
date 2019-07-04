using Base.Mvvm.Converter;
using Exercise.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Exercise.View
{
    internal class HasExceptionConverter : VisibilityConverter
    {
        internal HasExceptionConverter()
        {
            CollapsedValues = new object[] { 0 };
            //VisibleValues = new object[0];
        }
    }

    internal class NoExceptionConverter : VisibilityConverter
    {
        internal NoExceptionConverter()
        {
            VisibleValues = new object[] { 0 };
            //CollapsedValues = new object[0];
        }
    }

    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class SummaryPage : Page
    {

        public SummaryPage()
        {
            InitializeComponent();
        }

        private void PUButton_Click(object sender, RoutedEventArgs e)
        {
            SummaryViewModel vm = DataContext as SummaryViewModel;
            Window win = new Window()
            {
                Title = vm.ExerciseName + "扫描详情",
                DataContext = vm.ClassDetails,
                Content = FindResource("ClassDetails"),
                ShowInTaskbar = false,
                Width = 650, Height = 600,
                WindowStyle = WindowStyle.SingleBorderWindow, 
                ResizeMode = ResizeMode.NoResize
            };
            win.ShowDialog();
        }
    }
}
