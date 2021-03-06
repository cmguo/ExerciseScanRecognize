using Exercise.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using static Exercise.Model.ExerciseModel;

namespace Exercise.View.Resolve
{

    [ValueConversion(typeof(ExerciseModel.Exception), typeof(System.Windows.Controls.Page))]
    public class ExceptionPageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ExerciseModel.Exception ex = value as Model.ExerciseModel.Exception;
            if (ex == null)
                return null;
            switch (ex.Type)
            {
                case ExceptionType.NoPageCode:
                case ExceptionType.PageCodeMissMatch:
                case ExceptionType.AnalyzeException:
                    return null;
                case ExceptionType.NoStudentCode:
                case ExceptionType.StudentCodeMissMatch:
                    return new NoStudentCodePage() { DataContext = ex };
                case ExceptionType.AnswerException:
                case ExceptionType.CorrectionException:
                    return new AnswerExceptionPage() { DataContext = ex };
                case ExceptionType.PageLost:
                    return null;
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
