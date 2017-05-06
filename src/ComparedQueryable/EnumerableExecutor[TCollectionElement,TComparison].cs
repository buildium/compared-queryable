using System.Collections.Generic;
using System.Linq.Expressions;
using ComparedQueryable.NativeQueryable;

namespace ComparedQueryable
{
    public class EnumerableExecutor<TCollectionElement, TComparison> : EnumerableExecutor<TCollectionElement>
    {
        private readonly IComparer<TComparison> m_comparer;

        public EnumerableExecutor(Expression expression, IComparer<TComparison> comparer) : base(expression)
        {
            m_comparer = comparer;
        }

        internal override EnumerableRewriter GetEnumerableRewriter()
        {
            return new EnumerableRewriter<TComparison>(m_comparer);
        }
    }
}