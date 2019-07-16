using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TalBase.View
{
    public partial class TalRatio : RadioButton
    {

        public enum RatioStyles
        {
            Normal, 
            Solid,
            Fill
        }

        public static DependencyProperty RatioStyleProperty =
            DependencyProperty.Register("RatioStyle", typeof(RatioStyles), typeof(TalRatio), new PropertyMetadata(RatioStyles.Normal));

        public RatioStyles RatioStyle
        {
            get { return (RatioStyles)GetValue(RatioStyleProperty); }
            set { SetValue(RatioStyleProperty, value); }
        }

    }
}
