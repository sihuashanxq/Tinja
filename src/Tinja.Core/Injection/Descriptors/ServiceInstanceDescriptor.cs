using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceInstanceDescriptor : ServiceDescriptor
    {
        public object Instance { get; set; }
    }
}
