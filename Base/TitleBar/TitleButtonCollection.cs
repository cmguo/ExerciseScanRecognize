using System.Windows;

namespace Base.TitleBar
{
    public class TitleButtonCollection : FreezableCollection<TitleButton>
    {
        public TitleButtonCollection() { }

        public void ResolveGloabalButtons()
        {
            foreach (TitleButton b in this)
            {
                if (b.Content == null)
                {
                    b.Content = TitleBarManager.GetGlobalButton(b.Name);
                }
            }
        }
    }
}
