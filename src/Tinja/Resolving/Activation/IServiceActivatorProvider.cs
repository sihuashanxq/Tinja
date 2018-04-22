using System;
using Tinja.ServiceLife;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType);

        Func<IServiceResolver, IServiceLifeScope, object> Get(ServiceDependChain chain);
    }
}
