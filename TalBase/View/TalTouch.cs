using System.Windows;
using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalTouch : ButtonBase
    {

        public enum TouchStyles
        {
            Button,
            Label,
            Icon,
            Solid, 
            Fill
        }

        public static DependencyProperty TouchStyleProperty =
            DependencyProperty.Register("TouchStyle", typeof(TouchStyles), typeof(TalTouch), new PropertyMetadata(TouchStyles.Button));

        public TouchStyles TouchStyle
        {
            get { return (TouchStyles)GetValue(TouchStyleProperty); }
            set { SetValue(TouchStyleProperty, value); }
        }

        static TalTouch()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TalButton2), new FrameworkPropertyMetadata(typeof(TalButton2)));
        }

        public TalTouch()
        {
            Focusable = false;
        }

    }
}
