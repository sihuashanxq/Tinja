using System;
using System.Linq.Expressions;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation.Builder
{
    internal static class ActivatorUtil
    {
        internal delegate object ApplyLifeDelegate(Type serviceType, ServiceLifeStyle lifeStyle, IServiceLifeScope lifeScope, Func<IServiceResolver, object> factory);

        internal static ApplyLifeDelegate ApplyLifeFunc { get; }

        internal static ParameterExpression ParameterScope { get; }

        internal static ParameterExpression ParameterResolver { get; }

        internal static ConstantExpression ApplyLifeConstant { get; }

        static ActivatorUtil()
        {
            ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));
            ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

            ApplyLifeFunc = (serviceType, lifeStyle, scope, factory) => scope.GetOrAddResolvedService(serviceType, lifeStyle, factory);
            ApplyLifeConstant = Expression.Constant(ApplyLifeFunc, typeof(ApplyLifeDelegate));
        }
    }
}
