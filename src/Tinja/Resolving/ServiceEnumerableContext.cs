using System;
using System.Collections.Generic;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public class ServiceEnumerableContext : ServiceTypeContext
    {
        public List<IServiceContext> ElementContexts { get; }

        public ServiceEnumerableContext(Type serviceType, Type implementionType, ServiceLifeStyle lifeStyle, TypeConstructor[] constructors, List<IServiceContext> elementContexts) : base(serviceType, implementionType, lifeStyle, constructors)
        {
            ElementContexts = elementContexts;
        }
    }
}
