using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Extensions
{
    public static class ServiceResolverExtensions
    {
        public static IServiceResolver CreateScope(this IServiceResolver resolver)
        {
            return new ServiceResolver(resolver);
        }
    }
}
