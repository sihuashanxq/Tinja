using System;

namespace Tinja.Abstractions.Injection
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceDescriptor[] EmptyDesciptors = new ServiceDescriptor[0];
    }
}
