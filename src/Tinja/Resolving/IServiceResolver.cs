using System;

namespace Tinja.Resolving
{
    public interface IServiceResolver : IDisposable
    {
        object Resolve(Type serviceType);
    }
}
