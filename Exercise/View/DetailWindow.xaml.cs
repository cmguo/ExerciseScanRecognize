using Exercise.Model;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TalBase.View;
using static Exercise.Service.HistoryData;

namespace Exercise.View
{
    /// <summary>
    /// DetailWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DetailWindow : Window
    {
        public DetailWindow()
        {
            InitializeComponent();
            save.Visibility = ExerciseModel.Instance.ExerciseData == null ? Visibility.Collapsed : Visibility.Visible;
            exception.Visibility = ExerciseModel.Instance.Exceptions.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "保存";
            dlg.Filter = "表格|*.xlsx";
            dlg.FileName = "《" + ExerciseModel.Instance.ExerciseData.Title + "》成绩表.xlsx";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExerciseModel.Instance.SaveClassDetail(dlg.FileName);
                }
                catch (Exception ex)
                {
                    PopupDialog.Show(this, "发生错误", ex.Message, 0, "确定");
                }
            }
        }

    }

    [ValueConversion(typeof(StudentDetail), typeof(string))]
    public class StudentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StudentDetail student = value as StudentDetail;
            if (student == null)
                return null;
            if (double.IsNaN(student.Score))
                return student.Name;
            else
                return student.Name + " " + student.Score.ToString() + "分";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
