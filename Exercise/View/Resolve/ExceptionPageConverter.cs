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
                    return new NoPageCodePage() { DataContext = ex };
                case ExceptionType.NoStudentCode:
                    return new NoStudentCodePage() { DataContext = ex };
                case ExceptionType.AnswerException:
                    return new AnswerExceptionPage() { DataContext = ex };
                case ExceptionType.CorrectionException:
                    return new CorrectionExceptionPage() { DataContext = ex };
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
