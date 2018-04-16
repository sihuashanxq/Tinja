using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public interface IServiceInjectionActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeStyleScope, object> CreateActivator(ServiceDependChain chain);
    }
}
