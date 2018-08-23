using System;
using System.Threading.Tasks;
using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions.DynamicProxy.Configurations
{
    /// <summary>
    /// </summary>
    public interface IDynamicProxyConfiguration : IConfiguration
    {
        bool EnableInterfaceProxy { get; set; }

        bool EnableAstractionClassProxy { get; set; }

        IInterceptorRegistration ConfigureInterceptor(Type interceptorType);

        IInterceptorRegistration ConfigureInterceptor(Type interceptorType, ServiceLifeStyle lifeStyle);

        IInterceptorRegistration ConfigureInterceptor(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> hander);
    }
}
