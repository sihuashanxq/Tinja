using System;

namespace Tinja.Abstractions.Injection.Descriptors
{
    /// <summary>
    /// a descriptor for service
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>
        /// the type of service exported
        /// </summary>
        public Type ServiceType { get; set; }

        public long ServiceId { get; set; }

        /// <summary>
        /// the life style of service 
        /// </summary>
        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceDescriptor[] EmptyDesciptors = new ServiceDescriptor[0];
    }
}
