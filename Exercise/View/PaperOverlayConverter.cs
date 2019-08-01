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

namespace Exercise.View
{
    [ValueConversion(typeof(Page), typeof(Geometry))]
    public class PaperOverlayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<Location> locations;
            double inflate = 0.0;
            int radixs = 0;
            if ((string)parameter == "Result")
            {
                Page page = value as Page;
                if (page == null || page.Answer == null)
                    return null;
                inflate = 16.0;
                var items = page.Answer.AreaInfo.SelectMany(a => a.QuestionInfo.SelectMany(q => q.ItemInfo));
                locations = page.Answer.PaperMarkers.Select(m => m.MarkerLocation)
                    .Concat(page.Answer.AreaMarkers.Select(m => m.MarkerLocation))
                    .Concat(items.Where(i => i.AnalyzeResult != null).SelectMany(i => i.AnalyzeResult.Select(r => r.ValueLocation)));
            }
            else if ((string)parameter == "Exception")
            {
                radixs = 2;
                ItemException exception = value as ItemException;
                if (exception == null || exception.Answer.ItemLocation == null)
                    return null;
                locations = Enumerable.Repeat(exception.Answer.ItemLocation, 1);
            }
            else
            {
                return null;
            }
            IEnumerable<Geometry> geometries = locations.Where(l => l != null && l.LeftTop != null && l.RightBottom != null)
                .Select(l => new RectangleGeometry(MakeRect(l, inflate))
                {
                    RadiusX = radixs,
                    RadiusY = radixs,
                });
            return new GeometryGroup() { Children = new GeometryCollection(geometries) };
        }

        internal static Rect MakeRect(Location l, double inflate)
        {
            Rect rect = new Rect(l.LeftTop.X, l.LeftTop.Y, 
                l.RightBottom.X - l.LeftTop.X, l.RightBottom.Y - l.LeftTop.Y);
            rect.Inflate(inflate, inflate);
            return rect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
