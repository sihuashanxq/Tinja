using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceDelegateDescriptor : ServiceDescriptor
    {
        public Func<IServiceResolver, object> Delegate { get; set; }
    }
}
