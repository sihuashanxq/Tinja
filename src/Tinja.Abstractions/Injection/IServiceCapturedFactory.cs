using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceCapturedFactory
    {
        object CreateCapturedService(Func<IServiceResolver, object> factory);

        object CreateCapturedService(int serviceCacheId, Func<IServiceResolver, object> factory);
    }
}
