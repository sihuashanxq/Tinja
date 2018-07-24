using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceDescriptorFactory
    {
        ServiceDescriptor Create(Type serviceType);
    }
}
