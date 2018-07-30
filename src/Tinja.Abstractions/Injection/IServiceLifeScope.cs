using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        object GetOrAddResolvedService(object cacheKey, ServiceLifeStyle life, Func<IServiceResolver, object> factory);
    }
}
