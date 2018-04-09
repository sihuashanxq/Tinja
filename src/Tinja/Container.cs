using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.LifeStyle;
using Tinja.Resolving;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Chain;
using Tinja.Resolving.Context;
using Tinja.Resolving.Service;

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
