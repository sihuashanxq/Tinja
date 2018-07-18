using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions
{
    public interface IContainer
    {
        List<Action<IServiceConfiguration>> Configurators { get; }

        ConcurrentDictionary<Type, List<Component>> Components { get; }
    }
}
