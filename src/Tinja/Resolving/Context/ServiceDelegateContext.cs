using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceDelegateContext : IServiceContext
    {
        public Type ServiceType { get; }

        public ServiceLifeStyle LifeStyle { get; }

        public Func<IServiceResolver, object> ImplementionFactory { get; }

        public ServiceDelegateContext(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory)
        {
            LifeStyle = lifeStyle;
            ServiceType = serviceType;
            ImplementionFactory = factory;
        }
    }
}
