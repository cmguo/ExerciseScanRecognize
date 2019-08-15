using Panuon.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TalBase.View
{
    public partial class TalButton : PUButton
    {
        static TalButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalButton), new FrameworkPropertyMetadata(typeof(TalButton)));
        }

        public enum ButtonSizes
        {
            Small,
            Medium,
            Large
        }

        public ButtonSizes ButtonSize
        {
            get { return (ButtonSizes)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register("ButtonSize", typeof(ButtonSizes), typeof(TalButton), new PropertyMetadata(ButtonSizes.Medium));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(TalButton), new PropertyMetadata(false));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            internal set { SetValue(IsActiveProperty, value); }
        }


        private class FocusedStatus
        {

            private static Dictionary<Window, FocusedStatus> focusedStatus = new Dictionary<Window, FocusedStatus>();

            internal static FocusedStatus Get(TalButton b)
            {
                Window window = Window.GetWindow(b);
                if (window == null)
                    return null;
                FocusedStatus status = null;
                if (!focusedStatus.TryGetValue(window, out status))
                {
                    status = new FocusedStatus(window);
                    status.focused = Keyboard.FocusedElement;
                }
                status.Add(b);
                return status;
            }

            internal Window window;
            internal IInputElement focused;
            internal List<TalButton> buttons = new List<TalButton>();

            internal FocusedStatus(Window window)
            {
                this.window = window;
                focused = Keyboard.FocusedElement;
                window.GotKeyboardFocus += Window_PreviewGotKeyboardFocus;
                focusedStatus.Add(window, this);
            }

            private void Window_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                focused = e.NewFocus;
                //Debug.WriteLine(focused);
                if (focused == window)
                    return;
                foreach (TalButton b in buttons)
                {
                    b.IsActive = b.IsFocused || (b.IsDefault && !(focused is Button));
                    //Debug.WriteLine(b + " -> " + b.IsActive);
                }
            }

            internal void Add(TalButton b)
            {
                b.IsActive = b.IsFocused || (b.IsDefault && !(focused is Button));
                buttons.Add(b);
            }

            internal void Remove(TalButton b)
            {
                buttons.Remove(b);
                if (buttons.Count == 0)
                {
                    window.GotKeyboardFocus -= Window_PreviewGotKeyboardFocus;
                    focusedStatus.Remove(window);
                }
            }
        }

        private FocusedStatus status;

        public TalButton()
        {
            Loaded += TalButton_Loaded;
            Unloaded += TalButton_Unloaded;
        }

        private void TalButton_Loaded(object sender, RoutedEventArgs e)
        {
            status = FocusedStatus.Get(this);
        }

        private void TalButton_Unloaded(object sender, RoutedEventArgs e)
        {
            if (status != null)
                status.Remove(this);
        }

    }
}
