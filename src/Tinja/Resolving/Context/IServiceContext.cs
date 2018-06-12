using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public interface IServiceContext
    {
        Type ServiceType { get; }

        ServiceLifeStyle LifeStyle { get; }
    }
}
