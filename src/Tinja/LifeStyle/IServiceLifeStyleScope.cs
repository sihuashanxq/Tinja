using System;
using Tinja.Resolving.Context;

namespace Tinja.LifeStyle
{
    public interface IServiceLifeStyleScope : IDisposable
    {
        void ApplyLifeScope(Type serviceType, object instance, ServiceLifeStyle lifeStyle);

        object ApplyLifeScope(IResolvingContext context, Func<IResolvingContext, object> factory);
    }
}
