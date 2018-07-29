using System;

namespace Tinja.Abstractions.Injection.Descriptors
{
    public interface IServiceDescriptorFactory
    {
        ServiceDescriptor Create(Type serviceType);
    }
}
