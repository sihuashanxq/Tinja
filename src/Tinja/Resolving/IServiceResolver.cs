using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IDisposable
    {
        object Resolve(Type serviceType);

        IServiceLifeScope ServiceLifeScope { get; }
    }
}
