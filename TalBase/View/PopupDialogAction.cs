using Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Interactivity;
using Confirmation = TalBase.ViewModel.Confirmation;

namespace TalBase.View
{
    public class PopupDialogAction : TriggerAction<FrameworkElement>
    {

        protected override void Invoke(object parameter)
        {
            Confirmation confirmation = (parameter as InteractionRequestedEventArgs).Context as Confirmation;
            confirmation.Result = PopupDialog.Show(
                confirmation.Owner,
                confirmation.Title,
                confirmation.Message,
                confirmation.Image, 
                confirmation.Body,
                confirmation.DefaultButton, 
                confirmation.Buttons
            );
        }

    }

}

