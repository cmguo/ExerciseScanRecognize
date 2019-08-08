using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TalBase.View
{
    public partial class TalHyperlink : ButtonBase
    {
        static TalHyperlink()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TalHyperlink), new FrameworkPropertyMetadata(typeof(TalHyperlink)));
        }

        public TalHyperlink()
        {
            Focusable = false;
        }

    }
}
