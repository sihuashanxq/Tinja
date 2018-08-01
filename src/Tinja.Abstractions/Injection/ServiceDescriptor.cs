using System;

namespace Tinja.Abstractions.Injection
{
    /// <summary>
    /// a descriptor for service
    /// </summary>
    public class ServiceDescriptor
    {
        public int ServiceId { get; set; }

        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public ServiceDescriptor(int serviceId, Component component)
        {
            if (component == null)
            {
                throw new NullReferenceException(nameof(component));
            }

            ServiceId = serviceId;
            LifeStyle = component.LifeStyle;
            ServiceType = component.ServiceType;
            ImplementationType = component.ImplementationType;
            ImplementationFactory = component.ImplementationFactory;
            ImplementationInstance = component.ImplementationInstance;
        }
    }
}
