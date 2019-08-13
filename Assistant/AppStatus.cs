using Base.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Assistant
{
    public class AppStatus : NotifyBase
    {

        public static AppStatus Instance = new AppStatus();

        private bool _Busy = false;
        public bool Busy {
            get => _Busy;
            set
            {
                if (_Busy == value)
                    return;
                _Busy = value;
                SetTimer();
                RaisePropertyChanged("Busy");
            }
        }

        private bool _Free = false;
        public bool Free { get => _Free; private set { _Free = value; RaisePropertyChanged("Free"); } }

        private DispatcherTimer timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(5),
        };

        private AppStatus()
        {
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Busy)
                Free = true;
            timer.Stop();
        }

        private void SetTimer()
        {
            if (Busy)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
        }

    }
}
