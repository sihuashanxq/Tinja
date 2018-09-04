using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceCapturedFactory
    {
        object CreateCapturedService(Func<IServiceResolver, IServiceLifeScope, object> factory);

        object CreateCapturedService(int serviceCacheId, Func<IServiceResolver, IServiceLifeScope, object> factory);
    }
}
