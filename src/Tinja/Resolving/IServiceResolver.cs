using System;
using Tinja.LifeStyle;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeStyleScope LifeScope { get; }

        object Resolve(Type serviceType);
    }
}
