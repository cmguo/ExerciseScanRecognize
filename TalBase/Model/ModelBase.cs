using System.ComponentModel;
using System.Windows;

namespace TalBase.Model
{
    public class ModelBase : DependencyObject, INotifyPropertyChanged
    {
        protected void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
