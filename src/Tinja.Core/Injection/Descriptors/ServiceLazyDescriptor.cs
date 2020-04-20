using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceLazyDescriptor : ServiceDescriptor
    {
        public string Tag { get; set; }

        public bool TagOptional { get; set; }

        public Type ImplementationType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
