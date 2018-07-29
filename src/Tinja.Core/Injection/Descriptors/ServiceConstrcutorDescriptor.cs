using System;
using System.Reflection;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceConstrcutorDescriptor : ServiceDescriptor
    {
        public Type ImplementationType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementationType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
