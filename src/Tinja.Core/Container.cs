using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core
{
    public sealed class Container : IContainer
    {
        public ConcurrentDictionary<Type, List<ServiceEntry>> ServiceEntries { get; }

        public Container()
        {
            ServiceEntries = new ConcurrentDictionary<Type, List<ServiceEntry>>();
        }

        public IEnumerator<ServiceEntry> GetEnumerator()
        {
            return ServiceEntries.Values.SelectMany(item => item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
