using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalButton3 : ButtonBase
    {

        static TalButton3()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TalButton3), new FrameworkPropertyMetadata(typeof(TalButton3)));
        }

        public TalButton3()
        {
            Focusable = false;
        }

    }
}
