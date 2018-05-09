using System;

namespace Tinja.Resolving
{
    public class ServiceResolvingContext : IServiceResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public TypeMetadata ImplementationTypeMeta { get; }

        public ServiceResolvingContext(Type serviceType, TypeMetadata implementationTypeMeta, Component component)
        {
            Component = component;
            ServiceType = serviceType;
            ImplementationTypeMeta = implementationTypeMeta;
        }
    }
}
