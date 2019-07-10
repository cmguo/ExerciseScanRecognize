using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Application.Misc;
using Base.TitleBar;
using Panuon.UI;
using TalBase.ViewModel;

namespace Application
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : PUWindow
    {

        private Page lastPage;

        public MainWindow()
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            InitializeComponent();
            ScanDeviceSaraff.Init(this);
            Result = 0;
            frmMain.Navigating += FrmMain_Navigating;
            frmMain.Navigated += FrmMain_Navigated;
        }

        private void FrmMain_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (lastPage == null)
                return;
            ViewModelBase vm = lastPage.DataContext as ViewModelBase;
            vm.Release();
        }

        private void FrmMain_Navigated(object sender, NavigationEventArgs e)
        {
            Page page = e.Content as Page;
            showHeadOrTitle(page);
            if (lastPage != null)
                UnLoadNavButtons(lastPage);
            LoadNavButtons(page);
            if (IsLoaded)
                SetNavCommands(page);
            else
                Loaded += delegate
                {
                    SetNavCommands(page);
                };
            lastPage = page;
        }

        public void UnLoadNavButtons(Page page)
        {
            TitleButtonCollection buttons = TitleBarManager.GetButtons(page);
            if (buttons == null)
                return;
            buttons.ResolveGloabalButtons();
            foreach (TitleButton b in buttons)
            {
                if (b.Content == null)
                    continue;
                RemoveNavButton(b.Content);
                if (b.Content.DataContext == page.DataContext)
                    b.Content.DataContext = null;
                Button button = b.Content as Button;
                if (button != null && button.CommandParameter == page)
                    button.CommandParameter = null;
            }
        }

        public void LoadNavButtons(Page page)
        {
            TitleButtonCollection buttons = TitleBarManager.GetButtons(page);
            if (buttons != null)
            {
                buttons.ResolveGloabalButtons();
                foreach (TitleButton b in buttons)
                {
                    if (b.Content == null)
                        continue;
                    if (b.Content.DataContext == null)
                        b.Content.DataContext = page.DataContext;
                    b.Content.VerticalAlignment = VerticalAlignment.Center;
                    Button button = b.Content as Button;
                    if (button != null && button.CommandParameter == null)
                        button.CommandParameter = page;
                    AppendNavButton(b.Content);
                }
            }
        }

        public void SetNavCommands(Page page)
        {
            TitleCommandCollection commands = TitleBarManager.GetCommands(page);
            if (commands != null)
            {
                foreach (TitleCommand b in commands)
                {
                    PUButton button = GetNavButton(b.Name);
                    button.CommandBindings.Add(new CommandBinding(button.Command, (s, e) =>
                    {
                        object arg = b.CommandParameter;
                        if (arg == null)
                            arg = e;
                        b.Command.Execute(arg);
                    }, (s, e) =>
                    {
                        object arg = b.CommandParameter;
                        if (arg == null)
                            arg = e;
                        e.CanExecute = b.Command.CanExecute(arg);
                    }));
                }
            }
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
