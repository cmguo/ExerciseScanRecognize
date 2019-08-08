using System.Windows;
using System.Windows.Controls;

namespace TalBase.View
{
    public partial class TalText : TextBlock
    {

        public enum TextStyles
        {
            /*
             * 普通样式
             */
            Normal,
            /*
             * 标题样式
             */
            Weak,
            /*
             * 标题样式
             */
            Title,
            /*
             * 图标样式，大小16x16，正常时半透明，鼠标悬浮时高亮（不透明）
             */
            Fat,
            /*
             * 实心按键样式，鼠标悬浮、按下时都有不同背景
             */
            FatWeak,
            /*
             * 实心按键样式，鼠标悬浮、按下时都有不同背景
             */
            FatTitle,
            /*
             * 图标样式，大小16x16，正常时半透明，鼠标悬浮时高亮（不透明）
             */
            Large,
            /*
             * 实心按键样式，鼠标悬浮、按下时都有不同背景
             */
            Largest
        }

        public static DependencyProperty TextStyleProperty =
            DependencyProperty.Register("TextStyle", typeof(TextStyles), typeof(TalText), new PropertyMetadata(TextStyles.Normal));

        public TextStyles TextStyle
        {
            get { return (TextStyles)GetValue(TextStyleProperty); }
            set { SetValue(TextStyleProperty, value); }
        }

        static TalText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalText), new FrameworkPropertyMetadata(typeof(TalText)));
        }

    }
}
