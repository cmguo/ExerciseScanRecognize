using Base.Mvvm.Converter;
using System.Windows;
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

        public string Icon { get; set; }
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

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btn0)
                result = 0;
            else if (sender == btn1)
                result = 1;
            else
                result = 2;
            Close();
        }

        public int Popup()
        {
            if (Default == 0)
                btn0.Background = Brushes.Blue;
            else if (Default == 1)
                btn1.Background = Brushes.Blue;
            else if (Default == 2)
                btn2.Background = Brushes.Blue;
            if (Body != null)
                body.Children.Add(Body);
            ShowDialog();
            return result;
        }

        public static int Show(string msg, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Message = msg, Default = def }.Popup();
        }

        public static int Show(string msg, string icon, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Message = msg, Icon = icon, Default = def }.Popup();
        }

        public static int Show(string msg, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Message = msg, Body = body, Default = def }.Popup();
        }

        public static int Show(string msg, string icon, FrameworkElement body, int def, params string[] buttons)
        {
            return new PopupDialog(buttons) { Message = msg, Icon = icon, Body = body, Default = def }.Popup();
        }
    }
}
