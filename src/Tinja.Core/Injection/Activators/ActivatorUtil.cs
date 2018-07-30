using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activators
{
    internal static class ActivatorUtil
    {
        internal delegate object ApplyLifeDelegate(object cacheKey, ServiceLifeStyle lifeStyle, IServiceLifeScope lifeScope, Func<IServiceResolver, object> factory);

        internal static ApplyLifeDelegate ApplyLifeFunc { get; }

        internal static ParameterExpression ParameterScope { get; }

        internal static ParameterExpression ParameterResolver { get; }

        internal static ConstantExpression ApplyLifeConstant { get; }

        static ActivatorUtil()
        {
            ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));
            ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

            ApplyLifeFunc = (cacheKey, lifeStyle, scope, factory) => scope.GetOrAddResolvedService(cacheKey, lifeStyle, factory);
            ApplyLifeConstant = Expression.Constant(ApplyLifeFunc, typeof(ApplyLifeDelegate));
        }
    }
}
