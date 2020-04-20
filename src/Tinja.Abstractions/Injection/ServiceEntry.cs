using System;

namespace Tinja.Abstractions.Injection
{
    public class ServiceEntry
    {
        public string[] Tags { get; set; }

        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public ServiceEntry()
        {

        }

        public ServiceEntry Clone()
        {
            return new ServiceEntry()
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
