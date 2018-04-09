using Tinja.Resolving;

namespace Tinja.LifeStyle
{
    public class ServiceLifeStyleScopeFactory : IServiceLifeStyleScopeFactory
    {
        public IServiceLifeStyleScope Create(IServiceResolver resolver)
        {
            return new ServiceLifeStyleScope(resolver);
        }

        public IServiceLifeStyleScope Create(IServiceResolver resolver, IServiceLifeStyleScope scope)
        {
            return new ServiceLifeStyleScope(resolver, scope);
        }
    }
}
