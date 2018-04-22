using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeScope LifeScope { get; }

        object Resolve(Type serviceType);
    }
}
