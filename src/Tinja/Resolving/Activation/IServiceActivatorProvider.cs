using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public interface IServiceActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType);
    }
}
