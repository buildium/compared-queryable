// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ComparedQueryable.NativeQueryable
{
    // Must remain public for Silverlight
    public abstract class EnumerableExecutor
    {
        internal abstract object ExecuteBoxed();

        internal static EnumerableExecutor Create(Expression expression)
        {
            Type execType = typeof(EnumerableExecutor<>).MakeGenericType(expression.Type);
            return (EnumerableExecutor)Activator.CreateInstance(execType, expression);
        }
    }

    // Must remain public for Silverlight
    public class EnumerableExecutor<T> : EnumerableExecutor
    {
        private readonly Expression _expression;

        // Must remain public for Silverlight
        public EnumerableExecutor(Expression expression)
        {
            _expression = expression;
        }

        internal override object ExecuteBoxed() => Execute();

        internal T Execute()
        {
            EnumerableRewriter rewriter = GetEnumerableRewriter();
            Expression body = rewriter.Visit(_expression);
            Expression<Func<T>> f = Expression.Lambda<Func<T>>(body, (IEnumerable<ParameterExpression>)null);
            Func<T> func = f.Compile();
            return func();
        }

        internal virtual EnumerableRewriter GetEnumerableRewriter()
        {
            var rewriter = new EnumerableRewriter();
            return rewriter;
        }
    }
}