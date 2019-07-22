using Base.Misc;
using Base.Mvvm;
using Exercise.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using static Exercise.Service.HistoryData;
using TalBase.Utils;

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
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
            dataGrid.MouseLeftButtonDown += DataGrid_MouseLeftButtonDown;
            local.Items.SortDescriptions.Add(
                new SortDescription("ScanDate", ListSortDirection.Descending));
        }

        private void DataGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dataGrid.CommitEdit();
        }

        private void ButtonDetail_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataGridRow row = UITreeHelper.GetParentOfType<DataGridRow>(sender as UIElement);
            Record record = row.DataContext as Record;
            Window win = new DetailWindow()
            {
                Owner = UITreeHelper.GetParentOfType<Window>(this),
                Title = record.Name + "扫描详情",
                DataContext = record.DetailList,
            };
            e.Handled = true;
            win.ShowDialog();
        }

        private void ButtonRename_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataGridRow row = UITreeHelper.GetParentOfType<DataGridRow>(sender as UIElement);
            DataGridCellsPresenter presenter = UITreeHelper.GetChildOfType<DataGridCellsPresenter>(row);
            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);
            cell.Focus();
            cell.IsEditing = true;
            //dataGrid.BeginEdit();
            e.Handled = true;
        }

        private void ButtonPage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            object page = (sender as FrameworkElement).DataContext;
            HistoryViewModel vm = (DataContext as HistoryViewModel);
            vm.ShiftPages((bool)page == false);
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Record record = e.Row.DataContext as Record;
            HistoryViewModel vm = DataContext as HistoryViewModel;
            string old = record.Name;
            record.Name = UITreeHelper.GetChildOfType<TextBox>(e.EditingElement).Text;
            BackgroudWork.Execute(() => vm.ModifyRecordName(record, old));
        }

        private void ButtonRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HistoryViewModel vm = DataContext as HistoryViewModel;
            vm.PageIndex = 1;
        }

    }

    [ValueConversion(typeof(IList<ClassDetail>), typeof(string))]
    internal class HistroyDetailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList<ClassDetail> classes = value as IList<ClassDetail>;
            string result = "";
            int total = 0;
            foreach (ClassDetail c in classes)
            {
                result += "| " + c.Name + " " + c.SubmitStudentList.Count + "人";
                total += c.SubmitStudentList.Count;
            }
            result = "已读" + total + "份 " + result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class NoRecordNoficationContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value>0)
            {
                return Visibility.Collapsed;
            }
            if (NetWorkManager.CheckNetWorkAvailable())
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class ShowNetErrorContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value > 0)
            {
                return Visibility.Collapsed;
            }
            if (NetWorkManager.CheckNetWorkAvailable())
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
