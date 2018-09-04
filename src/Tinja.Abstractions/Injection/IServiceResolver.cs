using System;
using Tinja.Abstractions.Injection.Activations;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        IActivatorProvider Provider { get; }

        object ResolveService(Type serviceType);
    }
}
