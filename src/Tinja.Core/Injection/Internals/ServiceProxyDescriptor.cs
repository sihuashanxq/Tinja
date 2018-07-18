using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Internals
{
    public class ServiceProxyDescriptor : ServiceDescriptor
    {
        public ConstructorInfo[] Constrcutors => ProxyType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public Type ProxyType { get; set; }

        public Type TargetType { get; set; }
    }
}
