using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activations
{
    internal delegate object CreateTransientServiceDelegate(ServiceLifeScope scope, Func<IServiceResolver, ServiceLifeScope, object> factory);

    internal delegate object CreateScopedServiceDelegate(int serviceCacheId, ServiceLifeScope scope, Func<IServiceResolver, ServiceLifeScope, object> factory);

    internal static class ActivatorUtil
    {
        internal static ConstantExpression CreateCapturedScopedService { get; }

        internal static ConstantExpression CreateCapturedTransientServie { get; }

        internal static CreateScopedServiceDelegate CreateCapturedScopedServiceFunc { get; }

        internal static CreateTransientServiceDelegate CreateCapturedTransientServiceFunc { get; }

        static ActivatorUtil()
        {
            CreateCapturedScopedServiceFunc = (serviceId, scope, factory) => scope.CreateCapturedService(serviceId, factory);
            CreateCapturedTransientServiceFunc = (scope, factory) => scope.CreateCapturedService(factory);

            CreateCapturedScopedService = Expression.Constant(CreateCapturedScopedServiceFunc, typeof(CreateScopedServiceDelegate));
            CreateCapturedTransientServie = Expression.Constant(CreateCapturedTransientServiceFunc, typeof(CreateTransientServiceDelegate));
        }
    }
}
