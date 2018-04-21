using System;

namespace Tinja.Resolving.Context
{
    public class ResolvingContext : IResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public ServiceInfo ServiceInfo { get; }

        public ResolvingContext(Type serviceType, ServiceInfo serviceInfo, Component component)
        {
            Component = component;
            ServiceInfo = serviceInfo;
            ServiceType = serviceType;
        }
    }
}
