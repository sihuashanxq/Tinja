using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeScope Scope { get; }

        object Resolve(Type serviceType);
    }
}
