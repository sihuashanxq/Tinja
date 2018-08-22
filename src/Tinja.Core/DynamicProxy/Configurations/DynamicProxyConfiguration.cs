using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Configurations;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Injection;
using Tinja.Core.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Configurations
{
    public class DynamicProxyConfiguration : Configuration, IDynamicProxyConfiguration
    {
        public bool EnableInterfaceProxy
        {
            get => Get<bool>(Constant.EnableInterfaceProxyKey);
            set => Set(Constant.EnableInterfaceProxyKey, value);
        }

        public bool EnableAstractionClassProxy
        {
            get => Get<bool>(Constant.EnableAstractionClassProxyKey);
            set => Set(Constant.EnableAstractionClassProxyKey, value);
        }

        internal List<IInterceptorRegistration> InterceptorRegistrations { get; }

        public DynamicProxyConfiguration()
        {
            EnableInterfaceProxy = true;
            EnableAstractionClassProxy = true;
            InterceptorRegistrations = new List<IInterceptorRegistration>();
        }

        public IInterceptorRegistration ConfigureInterceptor(Type interceptorType)
        {
            var registration = new InterceptorTypeRegistration(interceptorType);

            InterceptorRegistrations.Add(registration);

            return registration;
        }

        public IInterceptorRegistration ConfigureInterceptor(Type interceptorType, ServiceLifeStyle lifeStyle)
        {
            var registration = new InterceptorTypeRegistration(interceptorType, lifeStyle);

            InterceptorRegistrations.Add(registration);

            return registration;
        }

        public IInterceptorRegistration ConfigureInterceptor(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> hander)
        {
            var registration = new InterceptorDelegateRegistration(hander);

            InterceptorRegistrations.Add(registration);

            return registration;
        }
    }
}
