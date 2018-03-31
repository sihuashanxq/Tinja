using System;
using System.Collections.Generic;

namespace Tinja.Resolving
{
    public interface IServiceResolver
    {
        object Resolve(Type serviceType);

        IEnumerable<object> ResolveAll(Type serviceType);
    }
}
