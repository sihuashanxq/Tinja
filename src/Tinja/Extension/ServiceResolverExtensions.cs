using Tinja.Resolving;

namespace Tinja
{
    public static class ServiceResolverExtensions
    {
        public static TType Resolve<TType>(this IServiceResolver serviceResolver)
        {
            return (TType)serviceResolver.Resolve(typeof(TType));
        }

        public static ServiceResolver CreateScope(this ServiceResolver serviceResolver)
        {
            return new ServiceResolver(
                serviceResolver.LifeStyleScope,
                serviceResolver.ContextBuilder
            );
        }
    }
}
