using Exercise.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Exercise.Model.ExerciseModel;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.View
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class ResolvePage : Page
    {

        public ResolvePage()
        {
            InitializeComponent();
            ResolveViewModel vm = DataContext as ResolveViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
            Loaded += (s, e) => vm.InitSelection();
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            frame.Navigating += Frame_Navigating;
        }

        private void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                e.Cancel = true;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Exception ex = e.NewValue as Exception;
            if (ex != null)
            {
                paper1.Scale = 0.666666;
                paper2.Scale = 0.666666;
                if (ex.Type == ExceptionType.AnalyzeException && ex.Page.Answer != null)
                {
                    face.IsChecked = true;
                    return;
                }
                face.IsChecked = false;
                while (frame.NavigationService.CanGoBack)
                    frame.NavigationService.RemoveBackEntry();
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResolveViewModel vm = sender as ResolveViewModel;
            if (e.PropertyName == "Selection")
            {
                treeView.Select(vm.Selection);
            }
        }

        internal void Resolve()
        {
            ResolveViewModel vm = DataContext as ResolveViewModel;
            vm.ResolveCommand.Execute(this);
        }

        internal void SetPaperFocusRect(Rect rect)
        {
            paper1.FocusRect = rect;
        }

        private void ButtonFace1_Click(object sender, RoutedEventArgs e)
        {
            paper1.Visibility = Visibility.Visible;
            paper2.Visibility = Visibility.Collapsed;
        }

        private void ButtonFace2_Click(object sender, RoutedEventArgs e)
        {
            paper1.Visibility = Visibility.Collapsed;
            paper2.Visibility = Visibility.Visible;
        }

        private void ButtonInc_Click(object sender, RoutedEventArgs e)
        {
            if (paper1.Scale < 2)
            {
                paper1.Scale *= 1.5;
                paper2.Scale *= 1.5;
            }
        }

        private void ButtonDec_Click(object sender, RoutedEventArgs e)
        {
            if (paper1.Scale > 0.7)
            {
                paper1.Scale /= 1.5;
                paper2.Scale /= 1.5;
            }
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class DuplexFaceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Exception ex = value as Exception;
            if (ex != null && ex.Page.Another != null
                && ex.Type != ExceptionType.AnswerException
                && ex.Type != ExceptionType.CorrectionException
                && ex.Type != ExceptionType.PageLost)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
