using System;
using System.Collections.Generic;

namespace Tinja.Resolving.Context
{
    public class ResolvingEnumerableContext : IResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public List<IResolvingContext> ElementContexts { get; }

        public ResolvingEnumerableContext(Type serviceType, Component component, List<IResolvingContext> elementContexts)
        {
            Component = component;
            ServiceType = serviceType;
            ElementContexts = elementContexts;
        }
    }
}
