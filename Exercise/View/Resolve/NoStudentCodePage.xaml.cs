using Base.Misc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Exercise.View.Resolve
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class NoStudentCodePage : System.Windows.Controls.Page
    {

        public NoStudentCodePage()
        {
            InitializeComponent();
            studentList.GotKeyboardFocus += StudentList_GotKeyboardFocus;
            studentList.LostKeyboardFocus += StudentList_LostKeyboardFocus;
        }

        private void StudentList_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //studentList.IsDropDownOpen = false;
        }

        private void StudentList_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //studentList.IsDropDownOpen = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (studentList.SelectedItem == null)
            {
                if (studentList.Text == null || studentList.Text == "")
                {
                    msg.Text = "请选择筛选列表中的学生";
                }
                else
                {
                    msg.Text = "输入的学生不在本校范围";
                }
                return;
            }
            ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
            rp.Resolve();
        }

        private void StudentList_TextChanged(object sender, TextChangedEventArgs e)
        {
            studentList.IsDropDownOpen = true;
        }
    }
}
