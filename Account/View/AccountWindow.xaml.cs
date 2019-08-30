using Account.ViewModel;
using Base.Misc;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Account
{
    /// <summary>
    /// PageAccount.xaml 的交互逻辑
    /// </summary>
    public partial class AccountWindow : Window
    {
        private static readonly Logger Log = Logger.GetLogger<AccountWindow>();

        public AccountWindow()
        {
            InitializeComponent();
            Unloaded += AccountWindow_Unloaded;
        }

        private void AccountWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            (Page.DataContext as AccountViewModel).Release();
        }

        private void Mini_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Popup popup = FindResource("LogoutPopup") as Popup;
            popup.IsOpen = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Window window = Application.Current.MainWindow;
            if (!window.IsVisible)
            {
                Log.d("Close MainWindow");
                window.Close();
            }
        }
    }
}
