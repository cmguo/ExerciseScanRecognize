using System.Windows;
using System.Windows.Controls;

namespace TalBase.View
{
    public partial class TalRatio : RadioButton
    {

        public enum RatioStyles
        {
            /*
             * 普通样式，有边框，鼠标悬浮时、按下、选中时改变边框颜色
             */
            Normal,
            /*
             * 实心样式，无边框，鼠标悬浮时、按下、选中时改变背景颜色
             */
            Solid,
            /*
             * 实心样式，有边框，鼠标悬浮时、按下、选中时改变背景、边框颜色
             */
            Fill
        }

        public static DependencyProperty RatioStyleProperty =
            DependencyProperty.Register("RatioStyle", typeof(RatioStyles), typeof(TalRatio), new PropertyMetadata(RatioStyles.Normal));

        public RatioStyles RatioStyle
        {
            get { return (RatioStyles)GetValue(RatioStyleProperty); }
            set { SetValue(RatioStyleProperty, value); }
        }
        static TalRatio()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalRatio), new FrameworkPropertyMetadata(typeof(TalRatio)));
        }


    }
}
