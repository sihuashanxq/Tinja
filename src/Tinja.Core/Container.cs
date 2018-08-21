using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions;
using Tinja.Abstractions.Configurations;
using Tinja.Abstractions.Injection;

namespace Tinja.Core
{
    public sealed class Container : IContainer
    {
        public ConcurrentDictionary<Type, List<Component>> Components { get; }

        public Container()
        {
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }
    }
}
