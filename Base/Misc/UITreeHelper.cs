using System;
using System.Windows;
using System.Windows.Media;

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

        public static object GetInheritProperty(DependencyObject child, DependencyProperty property)
        {
            DependencyObject e = child;
            while (e != null && e.ReadLocalValue(property) == DependencyProperty.UnsetValue)
            {
                e = LogicalTreeHelper.GetParent(e);
            }
            return e.ReadLocalValue(property);
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

        public static T GetChildOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = null;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject v = VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetChildOfType<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;

        }

        public static T GetParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject e = child;
            Type type = typeof(T);
            while (e != null && !type.IsInstanceOfType(e))
            {
                e = VisualTreeHelper.GetParent(e);
            }
            return e as T;
        }


    }
}
