using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceProxyContext : ServiceTypeContext
    {
        public Type ProxyType { get; }

        public TypeConstructor[] ProxyConstructors { get; }

        public ServiceProxyContext(
            Type serviceType,
            Type proxyType,
            Type implementionType,
            ServiceLifeStyle lifeStyle,
            TypeConstructor[] constructors,
            TypeConstructor[] proxyConstrcutors
        ) : base(serviceType, implementionType, lifeStyle, constructors)
        {
            ProxyType = proxyType;
            ProxyConstructors = proxyConstrcutors;
        }
    }
}
