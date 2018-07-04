using System;
using System.Reflection;

namespace Tinja.Resolving.Context
{
    public class ServiceProxyContext : ServiceContext
    {
        public ConstructorInfo[] Constrcutors => ProxyType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public Type ProxyType { get; set; }

        public Type TargetType { get; set; }
    }
}
