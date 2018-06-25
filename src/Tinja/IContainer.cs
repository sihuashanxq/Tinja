using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Configuration;

namespace Tinja
{
    public interface IContainer
    {
        List<Action<IServiceConfiguration>> Configurators { get; }

        ConcurrentDictionary<Type, List<Component>> Components { get; }
    }
}
