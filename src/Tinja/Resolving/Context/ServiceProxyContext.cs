using System;

namespace Tinja.Resolving.Context
{
    public class ServiceProxyContext : ServiceContext
    {
        public Type ProxyType { get; set; }

        public TypeConstructor[] ProxyConstructors { get; set; }
    }
}
