using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions
{
    public interface IContainer : IEnumerable<ServiceDescriptor>
    {
        ConcurrentDictionary<Type, List<ServiceDescriptor>> ServiceDescriptors { get; }
    }
}
