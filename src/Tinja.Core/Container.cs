using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.Injection;

namespace Tinja.Core
{
    public sealed class Container : IContainer
    {
        public List<Action<IServiceConfiguration>> Configurators { get; }

        public ConcurrentDictionary<Type, List<Component>> Components { get; }

        public Container()
        {
            Configurators = new List<Action<IServiceConfiguration>>();
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }
    }
}
