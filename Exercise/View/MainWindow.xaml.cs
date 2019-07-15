using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Base.TitleBar;
using Exercise.View;
using TalBase.ViewModel;

namespace Excecise
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private Page lastPage;
        private bool draging;
        private Point dragStart;

        public MainWindow()
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            InitializeComponent();
            frmMain.Navigating += FrmMain_Navigating;
            frmMain.Navigated += FrmMain_Navigated;
            Loaded += MainWindow_Loaded;
            titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseLeftButtonUp += TitleBar_MouseLeftButtonUp;
            titleBar.LostMouseCapture += TitleBar_LostMouseCapture;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new HomePage());
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
                RemoveNavButton(b.Content, b.Dock);
                if (b.Content.DataContext == page.DataContext)
                    b.Content.DataContext = null;
                ButtonBase button = b.Content as ButtonBase;
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
                    ButtonBase button = b.Content as ButtonBase;
                    if (button != null && button.CommandParameter == null)
                        button.CommandParameter = page;
                    AppendNavButton(b.Content, b.Dock);
                }
            }
        }

        private void RemoveNavButton(FrameworkElement content, TitleButton.Docks? dock)
        {
            if (dock == TitleButton.Docks.Left)
                left.Children.Remove(content);
            else
                right.Children.Remove(content);
        }

        private void AppendNavButton(FrameworkElement content, TitleButton.Docks? dock)
        {
            if (dock == TitleButton.Docks.Left)
            {
                content.Margin = new Thickness(0, 0, 32, 0);
                left.Children.Add(content);
            }
            else
            {
                content.Margin = new Thickness(0, 0, 32, 0);
                right.Children.Add(content);
            }
        }

        public void SetNavCommands(Page page)
        {
            TitleCommandCollection commands = TitleBarManager.GetCommands(page);
            if (commands != null)
            {
                foreach (TitleCommand b in commands)
                {
                    ButtonBase button = GetNavButton(b.Name);
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

        private ButtonBase GetNavButton(string name)
        {
            if (name == "Close")
                return close;
            else
                return null;
        }

        private void TitleBar_LostMouseCapture(object sender, MouseEventArgs e)
        {
            draging = false;
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draging = false;
            titleBar.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!draging)
                return;
            Point pt = PointToScreen(e.GetPosition(this));
            Left += pt.X - dragStart.X;
            Top += pt.Y - dragStart.Y;
            dragStart = pt;
            e.Handled = true;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStart = PointToScreen(e.GetPosition(this));
            draging = true;
            titleBar.CaptureMouse();
            e.Handled = true;
        }

        private void Mini_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
