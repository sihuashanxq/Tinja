using System;
using System.Linq;
using System.Reflection;
using Tinja.Annotations;

namespace Tinja.Resolving
{
    public class ServiceInfo
    {
        public Type Type { get; }

        public PropertyInfo[] Properties { get; }

        public ServiceConstructorInfo[] Constructors { get; }

        public ServiceInfo(Type serviceType)
        {
            Type = serviceType;

            Properties = serviceType
                .GetProperties()
                .Where(i => i.SetMethod != null && i.GetCustomAttribute<InjectAttribute>() != null)
                .ToArray();

            Constructors = serviceType
                .GetConstructors()
                .Select(i => new ServiceConstructorInfo(i, i.GetParameters()))
                .ToArray();
        }
    }
}
