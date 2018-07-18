using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        object Resolve(Type serviceType);

        IServiceLifeScope ServiceLifeScope { get; }
    }
}
