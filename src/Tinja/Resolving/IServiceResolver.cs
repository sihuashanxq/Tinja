using System;
using Tinja.LifeStyle;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        IServiceLifeStyleScope Scope { get; }
    }
}
