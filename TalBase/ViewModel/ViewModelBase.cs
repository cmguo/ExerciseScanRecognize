using Base.Mvvm;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows.Controls;

namespace TalBase.ViewModel
{
    public class ViewModelBase : NotifyBase
    {

        public InteractionRequest<Confirmation> ConfirmationRequest { get; } = new InteractionRequest<Confirmation>();

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
