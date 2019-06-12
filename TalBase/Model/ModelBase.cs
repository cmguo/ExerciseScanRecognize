using System.ComponentModel;

namespace TalBase.Model
{
    public class ModelBase : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
