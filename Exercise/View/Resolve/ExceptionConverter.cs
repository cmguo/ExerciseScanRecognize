using Exercise.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static Exercise.Model.ExerciseModel;
using Page = Exercise.Model.Page;

namespace Exercise.View.Resolve
{

    [ValueConversion(typeof(ExerciseModel.Exception), typeof(string))]
    public class ExceptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ExerciseModel.Exception ex = value as Model.ExerciseModel.Exception;
            if (ex == null)
                return null;
            Page page = ex.Page;
            switch (ex.Type)
            {
                case ExceptionType.NoPageCode:
                    return "未知试卷";
                case ExceptionType.PageCodeMissMatch:
                    return "未知试卷";
                case ExceptionType.NoStudentCode:
                    return String.Format("试卷 （{0}-{1}页）", page.PageIndex + 1, page.PageIndex + 2);
                case ExceptionType.AnalyzeException:
                case ExceptionType.AnswerException:
                case ExceptionType.CorrectionException:
                    return String.Format("{0} {1} （第{2}页）", page.Student.TalNo, page.Student.Name, page.PageIndex + 1);
                case ExceptionType.PageLost:
                    return String.Format("{0} {1} （{2} {3}页）", page.Student.TalNo, page.Student.Name, page.PageIndex + 1, page.PageIndex + 2);
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
