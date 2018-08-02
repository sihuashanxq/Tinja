using System;

namespace Tinja.Abstractions.Injection.Activations
{
    public interface IActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType);
    }
}
