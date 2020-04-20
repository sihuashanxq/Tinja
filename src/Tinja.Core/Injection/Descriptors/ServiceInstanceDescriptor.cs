using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceInstanceDescriptor : ServiceDescriptor
    {
        public object Instance { get; set; }
    }
}
