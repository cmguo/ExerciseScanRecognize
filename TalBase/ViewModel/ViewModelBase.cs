using System.ComponentModel;
using System.Windows;

namespace TalBase.ViewModel
{
    public class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        protected void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Release()
        {
        }

    }
}
