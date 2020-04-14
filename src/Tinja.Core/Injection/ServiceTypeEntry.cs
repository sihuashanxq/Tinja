using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceTypeEntry : ServiceEntry
    {
        public string[] Tags { get; set; }

        public Type ImplementationType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
