using System.Windows;
using System.Windows.Input;

namespace Base.TitleBar
{
    public class TitleCommandCollection : FreezableCollection<TitleCommand>
    {
        public TitleCommandCollection() { }

        public ICommand Find(string name)
        {
            foreach (var c in this)
            {
                if (c.Name == name)
                    return c.Command;
            }
            return null;
        }
    }
}
