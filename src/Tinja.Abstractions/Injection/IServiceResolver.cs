using System;
using Tinja.Abstractions.Injection.Activators;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeScope Scope { get; }

        IActivatorProvider Provider { get; }

        object Resolve(Type serviceType);
    }
}
