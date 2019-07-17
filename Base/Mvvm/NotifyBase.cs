using System.ComponentModel;

namespace Base.Mvvm
{
    public class NotifyBase : INotifyPropertyChanged
    {
        protected bool RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
                return true;
            }
            else
            {
                return false;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
