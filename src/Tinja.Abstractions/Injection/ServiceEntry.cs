using System;

namespace Tinja.Abstractions.Injection
{
    public abstract class ServiceEntry
    {
        public string Tag { get; set; }

        public int ServiceId { get; set; }

        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceEntry[] EmptyEntries = new ServiceEntry[0];
    }
}
