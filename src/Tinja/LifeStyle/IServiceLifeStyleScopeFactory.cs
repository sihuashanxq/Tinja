using Tinja.Resolving;

namespace Tinja.LifeStyle
{
    public interface IServiceLifeStyleScopeFactory
    {
        IServiceLifeStyleScope Create(IServiceResolver resolver);

        IServiceLifeStyleScope Create(IServiceResolver resolver, IServiceLifeStyleScope scope);
    }
}
