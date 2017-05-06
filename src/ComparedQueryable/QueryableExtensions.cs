using System.Collections.Generic;
using System.Linq;
using NaturalSort;

namespace ComparedQueryable
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> AsNaturalQueryable<T>(this IEnumerable<T> collection)
        {
            return collection.AsComparedQueryable(NaturalSortComparer.Instance);
        }

        public static IQueryable<T> AsComparedQueryable<T, TComparison>(this IEnumerable<T> collection,
            IComparer<TComparison> comparer)
        {
            return new EnumerableQuery<T, TComparison>(collection, comparer);
        }
    }
}