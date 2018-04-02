using System;

namespace Tinja.Resolving.Builder
{
    public interface IServiceFactoryBuilder
    {
        Func<IContainer, ILifeStyleScope, object> Build(IServiceNode serviceNode);

        Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType);
    }
}
