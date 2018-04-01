using System;

namespace Tinja.Resolving.Builder
{
    public interface IServiceFactoryBuilder
    {
        Func<IContainer,ILifeStyleScope,object> Build(ServiceFactoryBuildContext context);

        Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType);
    }
}
