using System;

namespace Tinja.Abstractions.Injection
{
    public abstract class ServiceDescriptor
    {
        public int ServiceId { get; set; }

        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceDescriptor[] EmptyDescriptors = new ServiceDescriptor[0];
    }
}
