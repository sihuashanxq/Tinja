using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeStyleScope, object> Get(Type serviceType);

        Func<IServiceResolver, IServiceLifeStyleScope, object> Get(ServiceDependChain chain);
    }
}
