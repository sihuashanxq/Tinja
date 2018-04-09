using Tinja.Resolving;

namespace Tinja
{
    public static class ServiceResolverExtensions
    {
        public static TType GetService<TType>(this IServiceResolver resolver)
        {
            return (TType)resolver.GetService(typeof(TType));
        }

        public static IServiceResolver CreateScope(this IServiceResolver resolver)
        {
            return new ServiceResolver(resolver);
        }
    }
}
