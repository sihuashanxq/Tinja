using System;

namespace Tinja.Abstractions.Injection
{
    /// <summary>
    /// a descriptor for service
    /// </summary>
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public ServiceDescriptor(Component component)
        {
            if (component == null)
            {
                throw new NullReferenceException(nameof(component));
            }

            LifeStyle = component.LifeStyle;
            ServiceType = component.ServiceType;
            ImplementationType = component.ImplementationType;
            ImplementationFactory = component.ImplementationFactory;
            ImplementationInstance = component.ImplementationInstance;
        }
    }
}
