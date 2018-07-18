using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceLifeScopeFactory : IServiceLifeScopeFactory
    {
        public IServiceLifeScope Create(IServiceResolver resolver)
        {
            return new ServiceLifeScope(resolver);
        }

        public IServiceLifeScope Create(IServiceResolver resolver, IServiceLifeScope scope)
        {
            return new ServiceLifeScope(resolver, scope);
        }
    }
}
