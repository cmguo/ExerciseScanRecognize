using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace TalBase.ViewModel
{
    public class Confirmation : INotification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public object Content { get => Message; set => Message = value as string; }
        public string Image { get; set; }
        public FrameworkElement Body { get; set; }
        public int DefaultButton { get; set; }
        public string[] Buttons { get; set; }
        public UIElement Owner { get; set; }

        internal int Result { get; set; }
    }
}
