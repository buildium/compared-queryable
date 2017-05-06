using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ComparedQueryable.NativeQueryable;
using Queryable = System.Linq.Queryable;

namespace ComparedQueryable
{
    internal class EnumerableRewriter<TComparison> : EnumerableRewriter
    {
        private readonly IComparer<TComparison> m_comparer;
        private static readonly ISet<string> ComparerFunctions = new HashSet<string>(new[]
        {
            nameof(Queryable.OrderBy),
            nameof(Queryable.OrderByDescending),
            nameof(Queryable.ThenBy),
            nameof(Queryable.ThenByDescending)
        });

        static EnumerableRewriter()
        {
        }

        public EnumerableRewriter(IComparer<TComparison> comparer)
        {
            m_comparer = comparer;
        }

        protected override ReadOnlyCollection<Expression> FixupQuotedArgs(MethodInfo mi,
            ReadOnlyCollection<Expression> argList)
        {
            var queryableArgs = base.FixupQuotedArgs(mi, argList);
            if (!ComparerFunctions.Contains(mi.Name))
            {
                return queryableArgs;
            }
            var operandExpression = argList
                .Where(expression => expression is UnaryExpression)
                .Cast<UnaryExpression>()
                .FirstOrDefault();

            if (operandExpression == null)
            {
                return queryableArgs;
            }
            if (operandExpression.Operand.Type.GetGenericArguments().All(type => type != typeof(TComparison)))
            {
                return queryableArgs;
            }
            return queryableArgs.Concat(new[] { Expression.Constant(m_comparer) }).ToList().AsReadOnly();
        }

        protected override MethodInfo GetGenericMethod(Type[] typeArgs,
            MethodInfo matchingMethodInfo,
            IEnumerable<MethodInfo> potentialMethodMatches)
        {
            if (typeArgs.All(type => type != typeof(TComparison)))
            {
                return matchingMethodInfo.MakeGenericMethod(typeArgs);
            }

            var methodInfoWithComparer = potentialMethodMatches
                .FirstOrDefault(MethodInfoHasIComparerParameter);
            return (methodInfoWithComparer ?? matchingMethodInfo).MakeGenericMethod(typeArgs);
        }

        private static bool MethodInfoHasIComparerParameter(MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Any(parameterInfo => parameterInfo.ParameterType.GetGenericTypeDefinition() == typeof(IComparer<>));
        }
    }
}