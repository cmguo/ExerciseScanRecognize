using Base.Misc;
using Base.Mvvm.Converter;
using Panuon.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TalBase.View
{
    internal class ContentVisiableConverter : VisibilityConverter
    {
        internal ContentVisiableConverter()
        {
            //CollapsedValues = new object[] { 0 };
            CollapsedValues = new object[] { null };
        }
    }

    /// <summary>
    /// PopupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PopupDialog : Window
    {

        public string Caption { get; set; }
        public string Image { get; set; }
        public string Message { get; set; }
        public FrameworkElement Body { set; get; }
        public string Button0 { get; set; }
        public string Button1 { get; set; }
        public string Button2 { get; set; }
        public int Default { get; set; }

        private int result = -1;

        public PopupDialog(string[] buttons)
        {
            InitializeComponent();
            //Owner = Application.Current.MainWindow;
            DataContext = this;
            if (buttons.Length > 0)
                Button0 = buttons[0];
            if (buttons.Length > 1)
                Button1 = buttons[1];
            if (buttons.Length > 2)
                Button2 = buttons[2];
            btn0.Click += Btn_Click;
            btn1.Click += Btn_Click;
            btn2.Click += Btn_Click;
        }


        private void MouseLeftBtnDown_Drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btn0)
                result = 0;
            else if (sender == btn1)
                result = 1;
            else if (sender == btn2)
                result = 2;
            else
                result = -1;
            if (Body != null)
                body.Children.Remove(Body);
            Close();
        }

        public int Popup()
        {
            Button dft = null;
            if (Default == 0)
                dft = btn0;
            else if (Default == 1)
                dft = btn1;
            else if (Default == 2)
                dft = btn2;
            if (dft != null)
            {
                dft.Focus();
            }
            if (Body != null)
                body.Children.Add(Body);
            ShowDialog();
            return result;
        }

        public static int Show(string title, string msg, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Caption = title, Message = msg, Default = def }.Popup();
        }

        public static int Show(string title, string msg, string icon, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Caption = title, Message = msg, Image = icon, Default = def }.Popup();
        }

        public static int Show(string title, string msg, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Caption = title, Message = msg, Body = body, Default = def }.Popup();
        }

        public static int Show(string title, string msg, string icon, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Caption = title, Message = msg, Image = icon, Body = body, Default = def }.Popup();
        }

        public static int Show(UIElement owner, string title, string msg, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Owner = UITreeHelper.GetParentOfType<Window>(owner), Caption = title, Message = msg, Default = def }.Popup();
        }

        public static int Show(UIElement owner, string title, string msg, string icon, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Owner = UITreeHelper.GetParentOfType<Window>(owner), Caption = title, Message = msg, Image = icon, Default = def }.Popup();
        }

        public static int Show(UIElement owner, string title, string msg, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Owner = UITreeHelper.GetParentOfType<Window>(owner), Caption = title, Message = msg, Body = body, Default = def }.Popup();
        }

        public static int Show(UIElement owner, string title, string msg, string icon, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Owner = UITreeHelper.GetParentOfType<Window>(owner), Caption = title, Message = msg, Image = icon, Body = body, Default = def }.Popup();
        }
    }
}
