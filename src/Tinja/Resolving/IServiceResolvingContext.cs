using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public interface IServiceResolvingContext
    {
        Type ServiceType { get; }

        Component Component { get; }

        TypeMetadata ImplementationMeta { get; }
    }

    public interface IServiceResolvingContext2
    {
        Type ServiceType { get; }

        Type ImplementionType { get; }

        ServiceLifeStyle LifeStyle { get; }

        Func<IServiceResolver, object> ImplementionFactory { get; }
    }

    public interface IServiceResolveContext3 : IServiceResolvingContext2
    {
        Type ProxyType { get; }

        Func<IServiceResolver, object> ProxyFactory { get; }
    }
}
