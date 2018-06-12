using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceTypeContext : IServiceContext
    {
        public Type ServiceType { get; }

        public Type ImplementionType { get; }

        public ServiceLifeStyle LifeStyle { get; }

        public TypeConstructor[] Constrcutors { get; }

        public ServiceTypeContext(Type serviceType, Type implementionType, ServiceLifeStyle lifeStyle, TypeConstructor[] constructors)
        {
            ServiceType = serviceType;
            ImplementionType = implementionType;
            Constrcutors = constructors;
            LifeStyle = lifeStyle;
        }
    }
}
