using Base.Misc;
using Base.Mvvm;
using Exercise.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            classList.DataContext = SchoolModel.Instance;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
            rp.Resolve();
        }
    }
}
