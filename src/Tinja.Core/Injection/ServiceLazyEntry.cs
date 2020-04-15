using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceLazyEntry : ServiceEntry
    {
        public Type ImplementationType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
