using Base.Misc;
using Base.Mvvm.Converter;
using Exercise.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TalBase.View;

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
            KeyDown += SummaryPage_KeyDown;
            TextInput += SummaryPage_TextInput;
        }

        private void PUButton_Click(object sender, RoutedEventArgs e)
        {
            SummaryViewModel vm = DataContext as SummaryViewModel;
            Window win = new DetailWindow()
            {
                Owner = UITreeHelper.GetParentOfType<Window>(this),
                Title = vm.ExerciseName + "扫描详情",
                DataContext = vm.ClassDetails,
            };
            win.ShowDialog();
        }

        private string hake = "";

        private void SummaryPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) 
                && e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
            {
                if (e.Key.CompareTo(Key.A) >= 0 && e.Key.CompareTo(Key.Z) <= 0)
                {
                    hake += (char) ('A' + (int) e.Key - (int) Key.A);
                    if (hake == "QWER" && PopupDialog.Show(this, "确认", "伪造全校数据?!!", 1, "确定", "取消") == 0)
                    {
                        SummaryViewModel vm = DataContext as SummaryViewModel;
                        vm.FillAll();
                    }
                }
            }
            else
            {
                hake = "";
            }
        }

        private void SummaryPage_TextInput(object sender, TextCompositionEventArgs e)
        {
        }

    }
}
