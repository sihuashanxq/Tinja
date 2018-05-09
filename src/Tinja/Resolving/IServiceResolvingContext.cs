using System;

namespace Tinja.Resolving
{
    public interface IServiceResolvingContext
    {
        Type ServiceType { get; }

        Component Component { get; }

        TypeMetadata ImplementationTypeMeta { get; }
    }
}
