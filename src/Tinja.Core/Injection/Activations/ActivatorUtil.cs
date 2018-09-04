using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activations
{
    internal delegate object CreateTransientServiceDelegate(IServiceLifeScope scope, Func<IServiceResolver, IServiceLifeScope, object> factory);

    internal delegate object CreateScopedServiceDelegate(int serviceCacheId, IServiceLifeScope scope, Func<IServiceResolver, IServiceLifeScope, object> factory);

    internal static class ActivatorUtil
    {
        internal static ConstantExpression CreateCapturedScopedService { get; }

        internal static ConstantExpression CreateCapturedTransientServie { get; }

        internal static CreateScopedServiceDelegate CreateCapturedScopedServiceFunc { get; }

        internal static CreateTransientServiceDelegate CreateCapturedTransientServiceFunc { get; }

        static ActivatorUtil()
        {
            CreateCapturedScopedServiceFunc = (serviceId, scope, factory) => scope.Factory.CreateCapturedService(serviceId, factory);
            CreateCapturedTransientServiceFunc = (scope, factory) => scope.Factory.CreateCapturedService(factory);

            CreateCapturedScopedService = Expression.Constant(CreateCapturedScopedServiceFunc, typeof(CreateScopedServiceDelegate));
            CreateCapturedTransientServie = Expression.Constant(CreateCapturedTransientServiceFunc, typeof(CreateTransientServiceDelegate));
        }
    }
}
