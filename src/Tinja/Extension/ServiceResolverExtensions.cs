using Tinja.Resolving;

namespace Tinja
{
    public static class ServiceResolverExtensions
    {
        public static TType Resolve<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.Resolve(typeof(TType));
        }

        public static IServiceResolver CreateScope(this IServiceResolver resolver)
        {
            return new ServiceResolver(resolver);
        }
    }
}
