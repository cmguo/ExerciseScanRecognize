using Base.Misc;
using Base.Mvvm;
using Base.Mvvm.Converter;
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

namespace Exercise.View
{
    /// <summary>
    internal class NonNullVisibleConverter : VisibilityConverter
    {
        internal NonNullVisibleConverter()
        {
            CollapsedValues = new object[] { null };
        }
    }

    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class ResolvePage : Page
    {

        public ResolvePage()
        {
            InitializeComponent();
        }

    }
}
