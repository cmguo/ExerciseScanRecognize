using Panuon.UI;
using System.Windows;

namespace TalBase.View
{
    public partial class TalButton : PUButton
    {
        public enum ButtonSizes
        {
            Small,
            Medium,
            Large
        }

        public ButtonSizes ButtonSize
        {
            get { return (ButtonSizes) GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty ButtonSizeProperty = 
            DependencyProperty.Register("ButtonSize", typeof(ButtonSizes), typeof(TalButton), new PropertyMetadata(ButtonSizes.Medium));

    }
}
