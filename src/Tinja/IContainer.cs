using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tinja
{
    public interface IContainer
    {
        IServiceConfiguration Configuration { get; }

        ConcurrentDictionary<Type, List<Component>> Components { get; }
    }
}
