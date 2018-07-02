using System;
using Tinja.Resolving;

namespace Tinja.ServiceLife
{
    public interface IServiceLifeScope : IDisposable
    {
        void AddResolvedService(object instance);

        object GetOrAddResolvedService(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory);
    }
}
