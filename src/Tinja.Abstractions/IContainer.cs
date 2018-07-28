using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions
{
    public interface IContainer
    {
        List<Action<IContainerConfiguration>> Configurators { get; }

        ConcurrentDictionary<Type, List<Component>> Components { get; }
    }
}
