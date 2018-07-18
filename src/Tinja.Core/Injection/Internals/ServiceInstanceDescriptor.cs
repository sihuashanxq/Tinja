using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Internals
{
    public class ServiceInstanceDescriptor : ServiceDescriptor
    {
        public object Instance { get; set; }
    }
}
