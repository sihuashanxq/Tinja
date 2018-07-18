using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceDescriptorFactory
    {
        ServiceDescriptor CreateDescriptor(Type serviceType);
    }
}
