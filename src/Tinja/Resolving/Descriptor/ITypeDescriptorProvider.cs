using System;

namespace Tinja.Resolving.Descriptor
{
    public interface ITypeDescriptorProvider
    {
        TypeDescriptor Get(Type type);
    }
}
