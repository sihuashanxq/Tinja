using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activators
{
    internal static class ActivatorUtil
    {
        internal delegate object ResolveServiceDelegate(IServiceLifeScope scope, Func<IServiceResolver, object> factory);

        internal delegate object ResolveCachedServiceDelegate(long serviceId, IServiceLifeScope scope, Func<IServiceResolver, object> factory);

        internal static ParameterExpression ParameterScope { get; }

        internal static ParameterExpression ParameterResolver { get; }

        internal static ConstantExpression ResolveServieConstant { get; }

        internal static ConstantExpression ResolveScopedServiceConstant { get; }

        internal static ConstantExpression ResolveSingletonServiceConstant { get; }

        internal static ResolveServiceDelegate ResolveService { get; }

        internal static ResolveCachedServiceDelegate ResolveScopedService { get; }

        internal static ResolveCachedServiceDelegate ResolveSingletonService { get; }

        static ActivatorUtil()
        {
            ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));
            ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

            ResolveService = (scope, factory) => scope.ResolveService(factory);
            ResolveScopedService = (serviceId, scope, factory) => scope.ResolveCachedService(serviceId, factory);
            ResolveSingletonService = (serviceId, scope, factory) => scope.ServiceRootScope.ResolveCachedService(serviceId, factory);

            ResolveServieConstant = Expression.Constant(ResolveService, typeof(ResolveServiceDelegate));
            ResolveScopedServiceConstant = Expression.Constant(ResolveScopedService, typeof(ResolveCachedServiceDelegate));
            ResolveSingletonServiceConstant = Expression.Constant(ResolveSingletonService, typeof(ResolveCachedServiceDelegate));
        }
    }
}
