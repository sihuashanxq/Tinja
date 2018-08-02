using System;
using Tinja.Abstractions.Injection.Activations;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeScope Scope { get; }

        IActivatorProvider Provider { get; }

        object ResolveService(Type serviceType);
    }
}
