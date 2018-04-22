using Tinja.Resolving;

namespace Tinja.ServiceLife
{
    public interface IServiceLifeScopeFactory
    {
        IServiceLifeScope Create(IServiceResolver resolver);

        IServiceLifeScope Create(IServiceResolver resolver, IServiceLifeScope scope);
    }
}
