using System;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.ServiceLife
{
    public interface IServiceLifeScope : IDisposable
    {
        object ApplyServiceLifeStyle(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory);

        object ApplyServiceLifeStyle(IResolvingContext context, Func<IServiceResolver, object> factory);
    }
}
