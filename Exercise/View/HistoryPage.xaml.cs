using Base.Misc;
using Base.Mvvm;
using Exercise.Service;
using Exercise.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using static Exercise.Service.HistoryData;

namespace Exercise.View
{
    /// <summary>
    /// HistoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryPage : Page
    {

        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof(string), typeof(HistoryPage));

        public ObservableCollection<string> Pages { get; private set; }
        public string PageIndex
        {
            get => GetValue(PageIndexProperty) as string;
            set => SetValue(PageIndexProperty, value);
        }

        private int pageStart = 1;
        private int pageEnd = 5;

        public HistoryPage()
        {
            Pages = new ObservableCollection<string>();
            InitializeComponent();
            DataContext = FindResource("ViewModel");
            (DataContext as HistoryViewModel).PropertyChanged += HistoryPage_PropertyChanged;
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
            dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
        }

        private void HistoryPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageCount")
            {
                HistoryViewModel vm = (DataContext as HistoryViewModel);
                pageStart = 1;
                if (vm.PageCount <= 5)
                {
                    for (int i = 1; i <= vm.PageCount; ++i)
                        Pages.Add(i.ToString());
                    pageEnd = vm.PageCount;
                }
                else
                {
                    Pages.Add("<");
                    for (int i = 1; i <= 5; ++i)
                        Pages.Add(i.ToString());
                    Pages.Add(">");
                    pageEnd = 5;
                }
                PageIndex = Pages[1];
            }
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
            Record record = row.DataContext as Record;
            cell.Focus();
            dataGrid.BeginEdit();
            e.Handled = true;
        }

        private void ButtonPage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string page = (string) (sender as FrameworkElement).DataContext;
            if (page == "<")
            {
                if (pageStart > 1)
                {
                    --pageStart;
                    --pageEnd;
                    Pages.RemoveAt(5);
                    Pages.Insert(1, pageStart.ToString());
                }
            }
            else if (page == ">")
            {
                HistoryViewModel vm = (DataContext as HistoryViewModel);
                if (pageEnd < vm.PageCount - 1)
                {
                    ++pageStart;
                    ++pageEnd;
                    Pages.RemoveAt(1);
                    Pages.Insert(5, pageEnd.ToString());
                }
            }
            else
            {
                HistoryViewModel vm = (DataContext as HistoryViewModel);
                vm.PageIndex = int.Parse(page) - 1;
                PageIndex = page;
            }
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            dataGrid.UnselectAll();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Record record = e.Row.DataContext as Record;
            HistoryViewModel vm = DataContext as HistoryViewModel;
            string old = record.Name;
            record.Name = (e.EditingElement as TextBox).Text;
            BackgroudWork.Execute(() => vm.ModifyRecordName(record, old));
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
}
