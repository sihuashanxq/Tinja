using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Configuration;

namespace Tinja
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
