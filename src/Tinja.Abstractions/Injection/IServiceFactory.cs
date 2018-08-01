using System;
namespace Tinja.Abstractions.Injection
{
    public interface IServiceFactory
    {
        object CreateService(Func<IServiceResolver, object> factory);

        object CreateService(int serviceId, Func<IServiceResolver, object> factory);
    }
}
