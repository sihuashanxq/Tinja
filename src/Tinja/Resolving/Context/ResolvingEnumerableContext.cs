using System;
using System.Collections.Generic;

namespace Tinja.Resolving.Context
{
    public class ResolvingEnumerableContext : IResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public ServiceInfo ServiceInfo { get; }

        public List<IResolvingContext> ElementContexts { get; }

        public ResolvingEnumerableContext(Type serviceType, ServiceInfo serviceInfo, Component component, List<IResolvingContext> elementContexts)
        {
            Component = component;
            ServiceInfo = serviceInfo;
            ServiceType = serviceType;
            ElementContexts = elementContexts;
        }
    }
}
