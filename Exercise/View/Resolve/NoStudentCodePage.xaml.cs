using Base.Misc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Exercise.Model.ExerciseModel;

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
            studentList.DropDownOpened += StudentList_DropDownOpened;
            Loaded += NoStudentCodePage_Loaded;
        }

        private void NoStudentCodePage_Loaded(object sender, RoutedEventArgs e)
        {
            Exception ex = DataContext as Exception;
            ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
            rp.SetPaperFocusRect(PaperOverlayConverter.MakeRect(ex.Page.Answer.QRCodeLocation, 0));
        }

        private void StudentList_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            studentList.IsDropDownOpen = true;
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

        private void StudentList_DropDownOpened(object sender, System.EventArgs e)
        {
            var textBox = Keyboard.FocusedElement as TextBox;
            if (textBox != null)
            {
                textBox.SelectionLength = 0;
            }
        }

    }
}
