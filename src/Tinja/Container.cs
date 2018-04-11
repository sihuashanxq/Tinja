using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tinja
{
    public class Container : IContainer
    {
        public ConcurrentDictionary<Type, List<Component>> Components { get; }

        public Container()
        {
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }
    }
}
