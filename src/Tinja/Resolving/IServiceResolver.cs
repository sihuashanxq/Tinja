using System;

namespace Tinja.Resolving
{
    public interface IServiceResolver
    {
        object Resolve(Type serviceType);
    }
}
