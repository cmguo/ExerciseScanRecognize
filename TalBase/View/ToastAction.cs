using Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Interactivity;
using TalBase.ViewModel;

namespace TalBase.View
{
    public class ToastAction : TriggerAction<FrameworkElement>
    {

        protected override void Invoke(object parameter)
        {
            Information information = (parameter as InteractionRequestedEventArgs).Context as Information;
            TalToast.Show(information.Message);
        }

    }

}

