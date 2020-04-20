using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceDescriptorFactory
    {
        ServiceDescriptor Create(Type serviceType, string tag, bool tagOptional);
    }
}