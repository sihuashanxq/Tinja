using System;
using Tinja.LifeStyle;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IDisposable
    {
        IServiceLifeStyleScope Scope { get; }

        object Resolve(Type serviceType);
    }
}
