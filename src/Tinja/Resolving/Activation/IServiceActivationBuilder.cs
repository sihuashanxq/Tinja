using System;
using Tinja.LifeStyle;
using Tinja.Resolving.Chain.Node;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivationBuilder
    {
        Func<IServiceResolver, IServiceLifeStyleScope, object> Build(IServiceChainNode chain);

        Func<IServiceResolver, IServiceLifeStyleScope, object> Build(Type resolvingType);
    }
}
