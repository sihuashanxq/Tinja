using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivationBuilder
    {
        Func<IServiceResolver, IServiceLifeStyleScope, object> Build(ServiceDependChain chain);

        Func<IServiceResolver, IServiceLifeStyleScope, object> Build(Type serviceType);
    }
}
