using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Extensions
{
    public static class ServiceScopeResolverExtensions
    {
        public static IServiceLifeScope CreateScope(this IServiceResolver resolver)
        {
            return resolver.ResolveServiceRequired<IServiceLifeScopeFactory>().CreateScope();
        }
    }
}
