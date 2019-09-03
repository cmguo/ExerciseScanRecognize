using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace TalBase.ViewModel
{
    public class Information : INotification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public object Content { get => Message; set => Message = value as string; }
        public string Image { get; set; }
    }
}
