using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceDelegateDescriptor : ServiceDescriptor
    {
        public Func<IServiceResolver, object> Delegate { get; set; }
    }
}
