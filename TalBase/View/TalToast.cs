using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace TalBase.View
{
    public partial class TalToast : ButtonBase
    {

        public static DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(TalToast));

        public enum ToastType
        {
            Ok,
            Warn,
        }

        public static DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(ToastType), typeof(TalToast));

        static TalToast()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TalButton2), new FrameworkPropertyMetadata(typeof(TalButton2)));
        }

        private static TalToast instance;

        public static void Show(string message)
        {
            instance.Message = message;
        }

        public string Message
        {
            get { return GetValue(MessageProperty) as string; }
            set
            {
                SetValue(MessageProperty, value);
                if (value == null)
                    return;
                if (value.StartsWith("X"))
                {
                    Type = ToastType.Warn;
                    value = value.Substring(1);
                }
                else
                {
                    Type = ToastType.Ok;
                }
                SetToastTimer();
            }
        }

        public ToastType Type
        {
            get { return (ToastType)GetValue(TypeProperty); }
            set
            {
                SetValue(TypeProperty, value);
            }
        }


        private DispatcherTimer timer;
        private int toastLife;


        public TalToast()
        {
            instance = this;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500), IsEnabled = true };
            timer.Tick += Timer_Tick;
        }

        private void SetToastTimer()
        {
            toastLife = 10; // 5 seconds
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (toastLife > 0 && --toastLife == 0)
                Message = null;
        }

    }
}
