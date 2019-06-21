using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Mvvm
{
    public static class ObservableCollections
    {

        public static ObservableCollection<O> Select2<I, O>(this ObservableCollection<I> origin, Func<I, O> selector)
        {
            ObservableCollection<O> result = new ObservableCollection<O>(origin.Select(selector));
            origin.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        result.Clear();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        int n1 = e.NewStartingIndex;
                        foreach (I i in e.NewItems)
                            result.Insert(n1++, selector(i));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        int n2 = e.OldItems.Count;
                        while (n2-- > 0)
                            result.RemoveAt(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        result.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException("Replace");
                }
            };
            return result;
        }

    }
}
