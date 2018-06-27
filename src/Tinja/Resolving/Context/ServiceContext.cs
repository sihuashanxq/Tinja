using System;
using Tinja.Resolving.Metadata;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceContext
    {
        public Type ServiceType { get; set; }

        public Type ImplementionType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public TypeConstructor[] Constrcutors { get; set; }

        public object ImplementionInstance { get; set; }

        public Func<IServiceResolver, object> ImplementionFactory { get; set; }
    }
}
