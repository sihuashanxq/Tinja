using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions
{
    public interface IContainer : IEnumerable<ServiceEntry>
    {
        ConcurrentDictionary<Type, List<ServiceEntry>> ServiceEntries { get; }
    }
}
