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

namespace Exercise.View.Resolve
{

    [ValueConversion(typeof(ExerciseModel.ExceptionList), typeof(string))]
    public class ExceptionListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ExerciseModel.ExceptionList el = value as Model.ExerciseModel.ExceptionList;
            if (el == null)
                return null;
            switch (el.Type)
            {
                case ExceptionType.NoPageCode:
                    return "无法识别的试卷";
                case ExceptionType.NoStudentCode:
                    return "待认领的试卷";
                case ExceptionType.AnswerException:
                    return "作答识别异常";
                case ExceptionType.CorrectionException:
                    return "批改识别异常";
                case ExceptionType.PageLost:
                    return "缺失的学生试卷";
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
