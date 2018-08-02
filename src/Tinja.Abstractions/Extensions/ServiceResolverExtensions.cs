using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions.Extensions
{
    public static class ServiceResolverExtensions
    {
        public static TType ResolveService<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.ResolveService(typeof(TType));
        }

        public static TType ResolveService<TType>(this IServiceResolver resolver, Type serviceType)
        {
            return (TType)resolver.ResolveService(serviceType);
        }

        public static object ResolveServiceRequired(this IServiceResolver resolver, Type serviceType)
        {
            return resolver.ResolveService(serviceType) ?? throw new InvalidOperationException();
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver resolver, Type serviceType)
        {
            return (TType)resolver.ResolveServiceRequired(serviceType);
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.ResolveServiceRequired(typeof(TType));
        }
    }
}
