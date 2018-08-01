using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceDelegateEntry : ServiceEntry
    {
        public Func<IServiceResolver, object> Delegate { get; set; }
    }
}
