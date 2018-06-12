using System;
using System.Collections.Generic;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceManyContext : ServiceTypeContext
    {
        public List<IServiceContext> ElementContexts { get; }

        public ServiceManyContext(Type serviceType, Type implementionType, ServiceLifeStyle lifeStyle, TypeConstructor[] constructors, List<IServiceContext> elementContexts) : base(serviceType, implementionType, lifeStyle, constructors)
        {
            ElementContexts = elementContexts;
        }
    }
}
