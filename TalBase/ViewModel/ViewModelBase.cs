using Base.Mvvm;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TalBase.ViewModel
{
    public class ViewModelBase : NotifyBase
    {

        protected void Navigate<P>(Page page) where P : Page
        {
            if (page.NavigationService != null)
                page.NavigationService.Navigate(Activator.CreateInstance(typeof(P)));
        }

        public virtual void Release()
        {
        }

    }
}
