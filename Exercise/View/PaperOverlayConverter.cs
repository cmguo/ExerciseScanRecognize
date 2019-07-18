using Exercise.Algorithm;
using Exercise.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static Exercise.Model.PageAnalyze;
using Exception = Exercise.Model.ExerciseModel.Exception;

namespace Exercise.View
{
    [ValueConversion(typeof(Page), typeof(Geometry))]
    public class PaperOverlayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<Location> locations;
            int radixs = 0;
            if ((string)parameter == "Result")
            {
                Page page = value as Page;
                if (page == null || page.Answer == null)
                    return null;
                var items = page.Answer.AreaInfo.SelectMany(a => a.QuestionInfo.SelectMany(q => q.ItemInfo));
                locations = items.Where(i => i.AnalyzeResult != null).SelectMany(i => i.AnalyzeResult.Select(r => r.ValueLocation));
            }
            else if ((string)parameter == "Exception")
            {
                radixs = 2;
                ItemException exception = value as ItemException;
                if (exception == null)
                    return null;
                locations = Enumerable.Repeat(exception.Answer.ItemLocation, 1);
            }
            else
            {
                return null;
            }
            IEnumerable<Geometry> geometries = locations.Where(l => l != null).Select(l => new RectangleGeometry(
                new Rect(l.LeftTop.X, l.LeftTop.Y, l.RightBottom.X - l.LeftTop.X, l.RightBottom.Y - l.LeftTop.Y))
            {
                RadiusX = radixs, RadiusY = radixs, 
            });
            return new GeometryGroup() { Children = new GeometryCollection(geometries) };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
