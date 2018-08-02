using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceCapturedFactory
    {
        object CreateCapturedService(Func<IServiceResolver, object> factory);

        object CreateCapturedService(int serviceId, Func<IServiceResolver, object> factory);
    }
}
