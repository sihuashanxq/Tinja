using System;
using System.Linq;
using System.Reflection;

namespace Tinja.Resolving
{
    public class ServiceInfo
    {
        public Type Type { get; }

        public PropertyInfo[] Properties { get; }

        public ServiceConstructorInfo[] Constructors { get; }

        public ServiceInfo(Type type)
        {
            Type = type;
            Properties = new PropertyInfo[0];
            Constructors = type
                .GetConstructors()
                .Select(i => new ServiceConstructorInfo(i, i.GetParameters()))
                .ToArray();
        }
    }
}
