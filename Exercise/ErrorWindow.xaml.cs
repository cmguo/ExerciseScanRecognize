using Base.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Base.Mvvm.RelayCommand;

namespace Exercise
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public static void Init()
        {
            new ErrorWindow();
        }

        public ErrorWindow()
        {
            InitializeComponent();
            RelayCommand.ActionException += RelayCommand_ActionException;
        }

        private void RelayCommand_ActionException(object sender, ActionExceptionEventArgs e)
        {
            Text.Text = e.Exception.ToString();
            e.IsHandled = true;
            Show();
        }
    }
}
