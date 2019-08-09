using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.Helpers
{
    public static class Enumerables
    {
        class FuncComparer<T> : IComparer<T>
        {
            private Func<T, T, int> comparer;

            public FuncComparer(Func<T, T, int> c)
            {
                comparer = c;
            }

            public int Compare(T x, T y)
            {
                return comparer(x, y);
            }
        };

        public static T MaxItem<T>(this IEnumerable<T> source, Func<T, T, int> comparer)
        {
            return source.OrderBy(t => t, new FuncComparer<T>((t1, t2) => comparer(t1, t2))).FirstOrDefault();
        }

        public static T MaxItem<T, TK>(this IEnumerable<T> source, Func<T, TK> selector)
        {
            return source.OrderBy(selector).FirstOrDefault();
        }

    }
}
