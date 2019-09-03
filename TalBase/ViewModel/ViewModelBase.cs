using Base.Mvvm;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Controls;

namespace TalBase.ViewModel
{
    public class ViewModelBase : NotifyBase
    {

        public InteractionRequest<Confirmation> ConfirmationRequest { get; } = new InteractionRequest<Confirmation>();

        public InteractionRequest<Information> InformationRequest { get; } = new InteractionRequest<Information>();


        protected int RaiseConfirmation(Confirmation confirmation)
        {
            ConfirmationRequest.Raise(confirmation);
            return confirmation.Result;
        }

        protected int RaiseConfirmation(string title, string message, int button, params string[] buttons)
        {
            return RaiseConfirmation(new Confirmation()
            {
                Title = title,
                Message = message,
                DefaultButton = button,
                Buttons = buttons,
                Owner = Application.Current.MainWindow
            });
        }

        protected int RaiseConfirmation(object context, string title, string message, int button, params string[] buttons)
        {
            return RaiseConfirmation(new Confirmation()
            {
                Title = title,
                Message = message,
                DefaultButton = button,
                Buttons = buttons,
                Owner = context as UIElement
            });
        }

        protected int RaiseConfirmation(object context, string title, string message, FrameworkElement body, int button, params string[] buttons)
        {
            return RaiseConfirmation(new Confirmation()
            {
                Title = title,
                Message = message,
                Body = body,
                DefaultButton = button,
                Buttons = buttons,
                Owner = context as UIElement
            });
        }


        protected void RaiseInformation(Information information)
        {
            InformationRequest.Raise(information);
        }

        protected void RaiseInformation(string information)
        {
            InformationRequest.Raise(new Information() { Message = information });
        }

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
