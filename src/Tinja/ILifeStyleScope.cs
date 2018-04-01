using System;
using Tinja.Resolving.ReslovingContext;

namespace Tinja
{
    public interface ILifeStyleScope : IDisposable
    {
        object GetOrAddLifeScopeInstance(IResolvingContext context, Func<IResolvingContext, object> factory);
    }
}
