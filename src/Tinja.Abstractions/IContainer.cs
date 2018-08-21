using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions
{
    public interface IContainer
    {
        ConcurrentDictionary<Type, List<Component>> Components { get; }
    }
}
