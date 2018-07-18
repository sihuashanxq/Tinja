using System;

namespace Tinja.Abstractions.Injection.Activators
{
    public interface IActivatorProvider
    {
        Func<IServiceResolver, IServiceLifeScope, object> Get(Type serviceType);
    }
}
