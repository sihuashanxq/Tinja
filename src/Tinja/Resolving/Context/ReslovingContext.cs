using System;

namespace Tinja.Resolving.Context
{
    public class ResolvingContext : IResolvingContext
    {
        public Type ServiceType { get; }

        public Component Component { get; }

        public ResolvingContext(Type resolvingType, Component component)
        {
            Component = component;
            ServiceType = resolvingType;
        }
    }
}
