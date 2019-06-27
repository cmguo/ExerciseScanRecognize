using System;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using Base.Misc;

namespace Base.TitleBar
{
    public class TitleBarManager
    {

        public static DependencyProperty ButtonsProperty =
            DependencyProperty.RegisterAttached("ButtonsInternal",
                                                typeof(TitleButtonCollection),
                                                typeof(TitleBarManager),
                                                new UIPropertyMetadata(null));

        public static TitleButtonCollection GetButtons(DependencyObject obj)
        {
            if (obj.GetValue(ButtonsProperty) == null)
            {
                obj.SetValue(ButtonsProperty, new TitleButtonCollection());
            }
            return (TitleButtonCollection)obj.GetValue(ButtonsProperty);
        }

        public static void SetButtons(DependencyObject obj, TitleButtonCollection value)
        {
            obj.SetValue(ButtonsProperty, value);
        }

    }
}
