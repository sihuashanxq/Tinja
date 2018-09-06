using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceResolver : IDisposable
    {
        object ResolveService(Type serviceType);
    }
}
