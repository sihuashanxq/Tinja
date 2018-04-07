using System;
using Tinja.Resolving.Context;

namespace Tinja
{
    public interface ILifeStyleScope : IDisposable
    {
        object ApplyLifeScope(IResolvingContext context, Func<IResolvingContext, object> factory);
    }
}
