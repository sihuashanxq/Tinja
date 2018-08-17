using System;

namespace Tinja.Abstractions.Injection
{
    public abstract class ServiceEntry
    {
        public Type ServiceType { get; set; }

        public int ServiceCacheId { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceEntry[] EmptyEntries = new ServiceEntry[0];
    }
}
