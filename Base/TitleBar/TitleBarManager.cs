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
            TitleButtonCollection buttons = (TitleButtonCollection)obj.GetValue(ButtonsProperty);
            if (buttons == null)
            {
                buttons = new TitleButtonCollection();
                SetButtons(obj, buttons);
            }
            return buttons;
        }

        public static void SetButtons(DependencyObject obj, TitleButtonCollection value)
        {
            obj.SetValue(ButtonsProperty, value);
        }

        public static DependencyProperty CommandsProperty =
            DependencyProperty.RegisterAttached("CommandsInternal",
                                                typeof(TitleCommandCollection),
                                                typeof(TitleBarManager),
                                                new UIPropertyMetadata(null));

        public static TitleCommandCollection GetCommands(DependencyObject obj)
        {
            TitleCommandCollection commands = (TitleCommandCollection)obj.GetValue(CommandsProperty);
            if (commands == null)
            {
                commands = new TitleCommandCollection();
                SetCommands(obj, commands);
            }
            return commands;
        }

        public static void SetCommands(DependencyObject obj, TitleCommandCollection value)
        {
            obj.SetValue(CommandsProperty, value);
        }

        public static DependencyProperty GlobalButtonsProperty =
            DependencyProperty.RegisterAttached("GlobalButtonsInternal",
                                                typeof(TitleButtonCollection),
                                                typeof(TitleBarManager),
                                                new UIPropertyMetadata(null, GlobalButtonsChanged));

        public static TitleButtonCollection GetGlobalButtons(DependencyObject obj)
        {
            TitleButtonCollection buttons =(TitleButtonCollection)obj.GetValue(GlobalButtonsProperty);
            if (buttons == null)
            {
                buttons = new TitleButtonCollection();
                SetGlobalButtons(obj, buttons);
            }
            return buttons;
        }

        public static void SetGlobalButtons(DependencyObject obj, TitleButtonCollection value)
        {
            obj.SetValue(GlobalButtonsProperty, value);
        }

        public static void GlobalButtonsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target != null)
            {
                TitleButtonCollection buttons = e.NewValue as TitleButtonCollection;
                foreach (TitleButton TitleButton in buttons)
                {
                    TitleButton TitleButtonClone = TitleButton.Clone() as TitleButton;
                    globalButtons.Add(TitleButtonClone);
                }
            }
        }

        private static TitleButtonCollection globalButtons = new TitleButtonCollection();

        internal static FrameworkElement GetGlobalButton(string name)
        {
            foreach (TitleButton b in globalButtons)
            {
                if (b.Name == name)
                    return b.Content;
            }
            return null;
        }

    }
}
