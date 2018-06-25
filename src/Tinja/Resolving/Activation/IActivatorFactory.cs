using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public interface IActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType);
    }
}
