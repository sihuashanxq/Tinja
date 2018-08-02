using System;

namespace Tinja.Abstractions.Injection.Activations
{
    public interface IActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType);
    }
}
