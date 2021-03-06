using Exercise.Model;
using System;
using System.Globalization;
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
            if ((string)parameter == "Tree")
            {
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
            if ((string)parameter == "Image")
            {
                switch (el.Type)
                {
                    case ExceptionType.NoPageCode:
                        return "/Icons/无法识别的试卷.svg";
                    case ExceptionType.NoStudentCode:
                        return "/Icons/待认领的试卷.svg";
                    case ExceptionType.AnswerException:
                        return "/Icons/作答识别异常.svg";
                    case ExceptionType.CorrectionException:
                        return "/Icons/批改识别异常.svg";
                    case ExceptionType.PageLost:
                        return "/Icons/缺失的学生试卷.svg";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Title")
            {
                switch (el.Type)
                {
                    case ExceptionType.PageLost:
                        return "以下试卷均缺失，可批量全部忽略";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "RemovePage")
            {
                switch (el.Type)
                {
                    case ExceptionType.PageLost:
                        return "忽略以上异常";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Ignore")
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
