using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Base.Mvvm.Converter;
using Panuon.UI;

namespace Account
{
    /// <summary>
    /// PageAccount.xaml 的交互逻辑
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();
        }

        private void Mini_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Window window = Application.Current.MainWindow;
            if (!window.IsActive)
                window.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Popup popup = FindResource("LogoutPopup") as Popup;
            popup.IsOpen = true;
        }
    }
}
