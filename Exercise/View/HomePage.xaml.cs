using Exercise.ViewModel;
using System.Windows.Controls;

namespace Exercise.View
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {

        public HomePage()
        {
            InitializeComponent();
            Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as HomeViewModel).CheckLocalCommand.Execute(this);
        }
    }
}
