using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        IServiceResolver ServiceResolver { get; }

        IServiceLifeScope ServiceRootScope { get; }

        object ResolveService(Func<IServiceResolver, object> factory);

        object ResolveCachedService(long cacheKey, Func<IServiceResolver, object> factory);
    }
}
