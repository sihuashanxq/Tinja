using System;
using Tinja.Resolving.Context;

namespace Tinja
{
    public interface ILifeStyleScope : IDisposable
    {
        object GetOrAddLifeScopeInstance(IResolvingContext context, Func<IResolvingContext, object> factory);

        object GetOrAddLifeScopeInstance2(Type instanceType, LifeStyle lifeStyle, Func<object> factory);
    }
}
