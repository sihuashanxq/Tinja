using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions.Extensions
{
    public static class ServiceResolverExtensions
    {
        public static TType ResolveService<TType>(this IServiceResolver serviceResolver)
        {
            return (TType)serviceResolver.ResolveService(typeof(TType));
        }

        public static TType ResolveService<TType>(this IServiceResolver serviceResolver, Type serviceType)
        {
            return (TType)serviceResolver.ResolveService(serviceType);
        }

        public static object ResolveServiceRequired(this IServiceResolver serviceResolver, Type serviceType)
        {
            return serviceResolver.ResolveService(serviceType) ?? throw new InvalidOperationException();
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver, Type serviceType)
        {
            return (TType)serviceResolver.ResolveServiceRequired(serviceType);
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver)
        {
            return (TType)serviceResolver.ResolveServiceRequired(typeof(TType));
        }
    }
}
