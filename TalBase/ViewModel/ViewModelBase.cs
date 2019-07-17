using Base.Mvvm;
using System.ComponentModel;
using System.Windows;

namespace TalBase.ViewModel
{
    public class ViewModelBase : NotifyBase
    {
        public virtual void Release()
        {
        }

    }
}
