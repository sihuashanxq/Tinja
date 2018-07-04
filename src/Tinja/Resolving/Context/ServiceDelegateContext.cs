using System;

namespace Tinja.Resolving.Context
{
    public class ServiceDelegateContext : ServiceContext
    {
        public Func<IServiceResolver, object> Delegate { get; set; }
    }
}
