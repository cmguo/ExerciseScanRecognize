using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Base.Misc
{
    public class UITreeHelper
    {
        public static Rect GetRenderBound(UIElement child, UIElement parent, Size size)
        {
            var t = child.TransformToAncestor(parent);
            Point p1 = t.Transform(new Point(0, 0));
            Point p2 = t.Transform(new Point(size.Width, size.Height));
            Point p3 = t.Transform(new Point(0, size.Height));
            Point p4 = t.Transform(new Point(size.Width, 0));
            Rect rc = new Rect(p1, p2);
            rc.Union(p3);
            rc.Union(p4);
            return rc;
        }

        public static DependencyObject GetParentWithProperty(DependencyObject child, DependencyProperty property)
        {
            DependencyObject e = child;
            while (e != null && e.ReadLocalValue(property) == DependencyProperty.UnsetValue)
            {
                e = LogicalTreeHelper.GetParent(e);
            }
            return e;
        }

        public static T GetParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject e = child;
            Type type = typeof(T);
            while (e != null && !type.IsInstanceOfType(e))
            {
                e = LogicalTreeHelper.GetParent(e);
            }
            return e as T;
        }


    }
}
