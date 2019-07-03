using System.Windows.Controls;

namespace Exercise.View
{
    /// <summary>
    /// HistoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryPage : Page
    {

        public HistoryPage()
        {
            InitializeComponent();
            DataContext = FindResource("ViewModel");
        }

    }
}
