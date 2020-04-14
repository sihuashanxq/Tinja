using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core
{
    public static class ServiceResolverExtensions
    {
        public static IServiceLifeScope CreateScope(this IServiceResolver resolver)
        {
            return resolver.ResolveServiceRequired<IServiceLifeScopeFactory>().CreateScope();
        }

        public static TType ResolveService<TType>(this IServiceResolver serviceResolver)
        {
            return (TType)serviceResolver.ResolveService(typeof(TType));
        }

        public static TType ResolveService<TType>(this IServiceResolver serviceResolver, string tag)
        {
            return (TType)serviceResolver.ResolveService(typeof(TType), tag);
        }

        public static TType ResolveService<TType>(this IServiceResolver serviceResolver, Type serviceType)
        {
            return (TType)serviceResolver.ResolveService(serviceType);
        }

        public static TType ResolveService<TType>(this IServiceResolver serviceResolver, Type serviceType, string tag)
        {
            return (TType)serviceResolver.ResolveService(serviceType, tag);
        }

        public static object ResolveServiceRequired(this IServiceResolver serviceResolver, Type serviceType)
        {
            return serviceResolver.ResolveService(serviceType) ?? throw new InvalidOperationException();
        }

        public static object ResolveServiceRequired(this IServiceResolver serviceResolver, Type serviceType, string tag)
        {
            return serviceResolver.ResolveService(serviceType, tag) ?? throw new InvalidOperationException();
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver, Type serviceType)
        {
            return (TType)serviceResolver.ResolveServiceRequired(serviceType);
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver, Type serviceType, string tag)
        {
            return (TType)serviceResolver.ResolveServiceRequired(serviceType, tag);
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver)
        {
            return (TType)serviceResolver.ResolveServiceRequired(typeof(TType));
        }

        public static TType ResolveServiceRequired<TType>(this IServiceResolver serviceResolver, string tag)
        {
            return (TType)serviceResolver.ResolveServiceRequired(typeof(TType), tag);
        }
    }
}
