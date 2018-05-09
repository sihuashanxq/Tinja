using System;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivatorFactory
    {
        Func<IServiceResolver, IServiceLifeScope, object> Create(ServiceDependencyChain chain);
    }
}
