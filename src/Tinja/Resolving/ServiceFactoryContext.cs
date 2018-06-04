using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public class ServiceFactoryContext : IServiceContext
    {
        public Type ServiceType { get; }

        public ServiceLifeStyle LifeStyle { get; }

        public Func<IServiceResolver, object> ImplementionFactory { get; }

        public ServiceFactoryContext(Type serviceType, ServiceLifeStyle lifeStyle, Func<IServiceResolver, object> factory)
        {
            LifeStyle = lifeStyle;
            ServiceType = serviceType;
            ImplementionFactory = factory;
        }
    }
}
