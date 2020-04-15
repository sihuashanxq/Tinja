using System;
using System.Linq.Expressions;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activations
{
    internal delegate object CreateTransientServiceDelegate(ServiceLifeScope scope, Func<IServiceResolver, ServiceLifeScope, object> factory);

    internal delegate object CreateScopedServiceDelegate(int serviceId, ServiceLifeScope scope, Func<IServiceResolver, ServiceLifeScope, object> factory);

    internal static class ActivatorUtil
    {
        internal static MethodInfo GetLazyValueFactoryMethod { get; }

        internal static MethodInfo GetTagLazyValueFactoryMethod { get; }

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

            GetLazyValueFactoryMethod = typeof(ActivatorUtil).GetMethod(nameof(GetLazyValueFactory), BindingFlags.Static | BindingFlags.Public);
            GetTagLazyValueFactoryMethod = typeof(ActivatorUtil).GetMethod(nameof(GetTagLazyValueFactory), BindingFlags.Static | BindingFlags.Public);
        }

        public static Func<TValue> GetLazyValueFactory<TValue>(IServiceResolver r)
        {
            return () => r.ResolveService<TValue>();
        }

        public static Func<TValue> GetTagLazyValueFactory<TValue>(IServiceResolver r,string tag)
        {
            return () => r.ResolveService<TValue>(tag);
        }

        public static MethodInfo MakeLazyValueFactoryMethod(Type valueType)
        {
            return GetLazyValueFactoryMethod.MakeGenericMethod(valueType);
        }

        public static MethodInfo MakeTagLazyValueFactoryMethod(Type valueType)
        {
            return GetTagLazyValueFactoryMethod.MakeGenericMethod(valueType);
        }
    }
}
