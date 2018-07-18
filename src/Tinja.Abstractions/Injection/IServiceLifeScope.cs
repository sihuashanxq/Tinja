using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        void AddResolvedService(object instance);

        object GetOrAddResolvedService(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory);
    }
}
