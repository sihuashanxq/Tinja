using System;

namespace Tinja.Abstractions.Injection
{
    /// <summary>
    /// a descriptor for service
    /// </summary>
    public class ServiceDescriptor
    {
        public string[] Tags { get; set; }

        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public ServiceDescriptor()
        {

        }

        public ServiceDescriptor Clone()
        {
            return new ServiceDescriptor()
            {
                Tags = Tags ?? new string[0],
                LifeStyle = LifeStyle,
                ServiceType = ServiceType,
                ImplementationType = ImplementationType,
                ImplementationInstance = ImplementationInstance,
                ImplementationFactory = ImplementationFactory
            };
        }
    }
}
