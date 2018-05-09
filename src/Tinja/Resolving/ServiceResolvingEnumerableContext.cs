using System;
using System.Collections.Generic;

namespace Tinja.Resolving
{
    public class ServiceResolvingEnumerableContext : IServiceResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public TypeMetadata ImplementationTypeMeta { get; }

        public List<IServiceResolvingContext> ElementContexts { get; }

        public ServiceResolvingEnumerableContext(Type serviceType, TypeMetadata implementationTypeMeta, Component component, List<IServiceResolvingContext> elementContexts)
        {
            Component = component;
            ServiceType = serviceType;
            ElementContexts = elementContexts;
            ImplementationTypeMeta = implementationTypeMeta;
        }
    }
}
