using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Internals
{
    public class ServiceConstrcutorDescriptor : ServiceDescriptor
    {
        public Type ImplementionType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementionType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
