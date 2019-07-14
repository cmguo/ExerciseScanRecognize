using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalButton2 : ButtonBase
    {

        static TalButton2()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TalButton2), new FrameworkPropertyMetadata(typeof(TalButton2)));
        }

        public TalButton2()
        {
            Focusable = false;
        }

    }
}
