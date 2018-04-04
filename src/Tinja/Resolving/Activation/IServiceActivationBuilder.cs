using System;
using Tinja.Resolving.Chain.Node;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivationBuilder
    {
        Func<IContainer, ILifeStyleScope, object> Build(IServiceChainNode chain);

        Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType);
    }
}
