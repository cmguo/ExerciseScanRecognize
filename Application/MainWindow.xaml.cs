using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application.Misc;
using Panuon.UI;

namespace Application
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : PUWindow
    {
   
        public MainWindow()
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            InitializeComponent();
            ScanDeviceSaraff.Init(this);
            LoadNavButtons();
            Result = 0;
            frmMain.Navigated += FrmMain_Navigated;
        }

        private void FrmMain_Navigated(object sender, NavigationEventArgs e)
        {
            showHeadOrTitle(e.Content as Page);
        }

        public void LoadNavButtons()
        {
            Thickness thickness = new Thickness();
            thickness.Left = 5;
            thickness.Right = 5;
            var btn1 = new PUButton()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Content = "试卷扫描记录管理",
                Padding = thickness,
                Margin =thickness,
                
            };
            var btn2 = new PUButton()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Content = "卢湾一中心小学",
                Padding = thickness,
                Margin = thickness,



            };
            AppendNavButton(btn1, new RoutedEventHandler((s, e) => { PUMessageBox.ShowDialog("点击了扫描试卷管理记录页面"); Result = 2; }), false);
            AppendNavButton(btn2, new RoutedEventHandler((s, e) => { PUMessageBox.ShowDialog("点击了学校管理，需要退出吗？"); Result = 1; }), false);
        }

        private void showHeadOrTitle(Page page)
        {
            if (Visibility.Visible == GetNavTitleBarVisibility(page))
            {
                Title = "读卷客户端";
            }
            else
            {
                var backBtn = new PUButton
                {
                    Content = new TextBlock
                    {
                        Text = "返回首页"
                    }
                };
                backBtn.Click += new RoutedEventHandler((s, e) => { PUMessageBox.ShowDialog("你点击返回到首页按钮!"); });
                Header = backBtn;
            }
        }

    }
}
