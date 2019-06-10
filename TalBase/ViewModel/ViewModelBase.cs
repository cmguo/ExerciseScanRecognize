using System.ComponentModel;

namespace TalBase.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
