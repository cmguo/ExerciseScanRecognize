using Exercise.Algorithm;
using Exercise.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.View
{
    [ValueConversion(typeof(Page), typeof(Geometry))]
    public class PaperOverlayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Page page = value as Page;
            if (page == null && value is Exception)
            {
                Exception ex = value as Exception;
                page = ex.Page;
                if (ex.Type == ExerciseModel.ExceptionType.AnswerException)
                    parameter = "AnswerException";
                else if (ex.Type == ExerciseModel.ExceptionType.CorrectionException)
                    parameter = "CorrectionException";
            }
            if (page == null || page.Answer == null)
                return null;
            IEnumerable<Location> locations;
            if ((string)parameter == "Result")
            {
                var items = page.Answer.AreaInfo.SelectMany(a => a.QuestionInfo.SelectMany(q => q.ItemInfo));
                locations = items.SelectMany(i => i.AnalyzeResult.Select(r => r.ValueLocation));
            }
            else if ((string)parameter == "AnswerException")
            {
                locations = page.Answer.AnswerExceptions.SelectMany(q => q.ItemInfo.Select(i => i.ItemLocation));
            }
            else if ((string)parameter == "CorrectionException")
            {
                locations = page.Answer.CorrectionExceptions.SelectMany(q => q.ItemInfo.Select(i => i.ItemLocation));
            }
            else
            {
                return null;
            }
            IEnumerable<Geometry> geometries = locations.Where(l => l != null).Select(l => new RectangleGeometry(
                new Rect(l.LeftTop.X, l.LeftTop.Y, l.RightBottom.X - l.LeftTop.X, l.RightBottom.Y - l.LeftTop.Y)));
            return new GeometryGroup() { Children = new GeometryCollection(geometries) };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
