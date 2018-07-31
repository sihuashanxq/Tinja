using System;
using System.Linq.Expressions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activators
{
    internal static class ActivatorUtil
    {
        internal delegate object CreateTransientServiceDelegate(IServiceLifeScope scope, Func<IServiceResolver, object> factory);

        internal delegate object CreateScopedServiceDelegate(long serviceId, IServiceLifeScope scope, Func<IServiceResolver, object> factory);

        internal static ParameterExpression ParameterScope { get; }

        internal static ParameterExpression ParameterResolver { get; }

        internal static ConstantExpression CreateScopedServiceConstant { get; }

        internal static ConstantExpression CreateTransientServieConstant { get; }

        internal static CreateScopedServiceDelegate CreateScopedService { get; }

        internal static CreateTransientServiceDelegate CreateTransientService { get; }

        static ActivatorUtil()
        {
            ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));
            ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

            CreateTransientService = (scope, factory) => scope.Factory.CreateService(factory);
            CreateScopedService = (serviceId, scope, factory) => scope.Factory.CreateService(serviceId, factory);

            CreateTransientServieConstant = Expression.Constant(CreateTransientService, typeof(CreateTransientServiceDelegate));
            CreateScopedServiceConstant = Expression.Constant(CreateScopedService, typeof(CreateScopedServiceDelegate));
        }
    }
}
