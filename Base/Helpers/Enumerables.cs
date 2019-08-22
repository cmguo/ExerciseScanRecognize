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

        public static T MinItem<T>(this IEnumerable<T> source, Func<T, T, int> comparer)
        {
            return source.Count() == 0 ? default(T) : source.Aggregate((t1, t2) => comparer(t1, t2) < 0 ? t1 : t2);
        }

        public static T MinItem<T, TK>(this IEnumerable<T> source, Func<T, TK> selector)
        {
            IComparer<TK> comparer = Comparer<TK>.Default;
            return source.Count() == 0 ? default(T) : source.Aggregate((t1, t2) => comparer.Compare(selector(t1), selector(t2)) < 0 ? t1 : t2);
        }

        public static T MaxItem<T>(this IEnumerable<T> source, Func<T, T, int> comparer)
        {
            return source.Count() == 0 ? default(T) : source.Aggregate((t1, t2) => comparer(t1, t2) > 0 ? t1 : t2);
        }

        public static T MaxItem<T, TK>(this IEnumerable<T> source, Func<T, TK> selector)
        {
            IComparer<TK> comparer = Comparer<TK>.Default;
            return source.Count() == 0 ? default(T) : source.Aggregate((t1, t2) => comparer.Compare(selector(t1), selector(t2)) > 0 ? t1 : t2);
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(s => s);
        }

        public static IEnumerable<T> SortDescending<T>(this IEnumerable<T> source)
        {
            return source.OrderByDescending(s => s);
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, Func<T, T, int> comparer)
        {
            return source.OrderBy(s => s, new FuncComparer<T>(comparer));
        }

        public static IEnumerable<T> SortDescending<T>(this IEnumerable<T> source, Func<T, T, int> comparer)
        {
            return source.OrderByDescending(s => s, new FuncComparer<T>(comparer));
        }

    }
}
