using System;

namespace Tinja.Abstractions.Injection.Extensions
{
    public static class ServiceResolverExtensions
    {
        public static TType Resolve<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.Resolve(typeof(TType));
        }

        public static TType Resolve<TType>(this IServiceResolver resolver, Type serviceType)
        {
            return (TType)resolver.Resolve(serviceType);
        }

        public static object ResolveRequired(this IServiceResolver resolver, Type serviceType)
        {
            return resolver.Resolve(serviceType) ?? throw new InvalidOperationException();
        }

        public static TType ResolveRequired<TType>(this IServiceResolver resolver, Type serviceType)
        {
            return (TType)resolver.ResolveRequired(serviceType);
        }

        public static TType ResolveRequired<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.ResolveRequired(typeof(TType));
        }

        public static IServiceResolver CreateScope(this IServiceResolver resolver)
        {
            return new ServiceResolver(resolver);
        }
    }
}
