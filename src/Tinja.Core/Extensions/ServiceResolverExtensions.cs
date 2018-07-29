using Tinja.Abstractions.Injection;
using Tinja.Core.Injection;

namespace Tinja.Core.Extensions
{
    public static class ServiceResolverExtensions
    {
        public static IServiceResolver CreateScope(this IServiceResolver resolver)
        {
            return new ServiceResolver(resolver);
        }
    }
}
