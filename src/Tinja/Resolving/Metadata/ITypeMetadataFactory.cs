using System;

namespace Tinja.Resolving.Metadata
{
    public interface ITypeMetadataFactory
    {
        TypeMetadata Create(Type type);
    }
}
