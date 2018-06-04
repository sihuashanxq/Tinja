using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public interface IServiceContext
    {
        Type ServiceType { get; }

        ServiceLifeStyle LifeStyle { get; }
    }
}
