using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors.Internals
{
    public class ServiceEntry
    {
        public long ServiceId { get; set; }

        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public ServiceEntry(long serviceId, Component component)
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
