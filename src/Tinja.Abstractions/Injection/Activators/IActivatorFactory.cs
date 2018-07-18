using System;

namespace Tinja.Abstractions.Injection.Activators
{
    public interface IActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType);
    }
}
