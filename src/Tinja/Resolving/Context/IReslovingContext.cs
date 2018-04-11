using System;

namespace Tinja.Resolving.Context
{
    public interface IResolvingContext
    {
        Type ServiceType { get; }

        Component Component { get; }
    }
}
