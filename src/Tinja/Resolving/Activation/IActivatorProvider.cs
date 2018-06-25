using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public interface IActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType);
    }
}
