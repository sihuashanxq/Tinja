using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activations
{
    internal delegate object CreateTransientServiceDelegate(IServiceLifeScope scope, Func<IServiceResolver, object> factory);

    internal delegate object CreateScopedServiceDelegate(int serviceId, IServiceLifeScope scope, Func<IServiceResolver, object> factory);

    internal static class ActivatorUtil
    {
        internal static ParameterExpression ParameterScope { get; }

        internal static ParameterExpression ParameterResolver { get; }

        internal static ConstantExpression CreateCapturedScopedService { get; }

        internal static ConstantExpression CreateCapturedTransientServie { get; }

        internal static CreateScopedServiceDelegate CreateCapturedScopedServiceFunc { get; }

        internal static CreateTransientServiceDelegate CreateCapturedTransientServiceFunc { get; }

        static ActivatorUtil()
        {
            ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));
            ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

            CreateCapturedScopedServiceFunc = (serviceId, scope, factory) => scope.Factory.CreateCapturedService(serviceId, factory);
            CreateCapturedTransientServiceFunc = (scope, factory) => scope.Factory.CreateCapturedService(factory);

            CreateCapturedScopedService = Expression.Constant(CreateCapturedScopedServiceFunc, typeof(CreateScopedServiceDelegate));
            CreateCapturedTransientServie = Expression.Constant(CreateCapturedTransientServiceFunc, typeof(CreateTransientServiceDelegate));
        }
    }
}
