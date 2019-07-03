using Exercise.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResolveViewModel vm = sender as ResolveViewModel;
            if (e.PropertyName == "Selection")
            {
                var tvi = FindItem(treeView, vm.Selection);
                if (tvi != null)
                {
                    tvi.IsSelected = true;
                }
                ButtonFace1_Click(this, null);
            }
        }

        private TreeViewItem FindItem(ItemsControl container, object item)
        {
            var c = container.ItemContainerGenerator.ContainerFromItem(item);
            if (c != null)
                return c as TreeViewItem;
            foreach (var i in container.ItemContainerGenerator.Items)
            {
                var cc = container.ItemContainerGenerator.ContainerFromItem(i);
                c = FindItem(cc as ItemsControl, item);
                if (c != null)
                    return c as TreeViewItem;
            }
            return null;
        }

        internal void Resolve()
        {
            ResolveViewModel vm = DataContext as ResolveViewModel;
            vm.ResolveCommand.Execute(this);
        }

        private PaperViewer viewer;

        private void ButtonFace1_Click(object sender, RoutedEventArgs e)
        {
            face1.Background = Brushes.Blue;
            face2.Background = Brushes.White;
            viewer = paper1;
            paper1.Visibility = Visibility.Visible;
            paper2.Visibility = Visibility.Collapsed;
        }

        private void ButtonFace2_Click(object sender, RoutedEventArgs e)
        {
            face1.Background = Brushes.White;
            face2.Background = Brushes.Blue;
            viewer = paper2;
            paper1.Visibility = Visibility.Collapsed;
            paper2.Visibility = Visibility.Visible;
        }

        private void ButtonInc_Click(object sender, RoutedEventArgs e)
        {
            viewer.Scale *= 1.5;
        }

        private void ButtonDec_Click(object sender, RoutedEventArgs e)
        {
            viewer.Scale /= 1.5;
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
                && ex.Type != ExceptionType.CorrectionException)
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
