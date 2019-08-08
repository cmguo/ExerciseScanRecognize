using System.Windows;
using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalTouch : ButtonBase
    {

        public enum TouchStyles
        {
            /*
             * 普通按钮样式，鼠标悬浮时改变背景
             */
            Button,
            /*
             * 文字样式，正常时半透明，鼠标悬浮时高亮（不透明）
             */
            Label,
            /*
             * 图标样式，大小16x16，正常时半透明，鼠标悬浮时高亮（不透明）
             */
            Icon,
            /*
             * 实心按键样式，鼠标悬浮、按下时都有不同背景
             */
            Solid,
            /*
             * 实心y有边框按键样式，鼠标悬浮、按下时都有不同背景
             */
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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalTouch), new FrameworkPropertyMetadata(typeof(TalTouch)));
        }

        public TalTouch()
        {
            Focusable = false;
        }

    }
}
