using System;
using Tinja.ServiceLife;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public interface IServiceInjectionActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(ServiceDependChain chain);
    }
}
