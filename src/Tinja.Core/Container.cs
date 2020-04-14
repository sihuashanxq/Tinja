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
        public ConcurrentDictionary<Type, List<ServiceDescriptor>> ServiceDescriptors { get; }

        public Container()
        {
            ServiceDescriptors = new ConcurrentDictionary<Type, List<ServiceDescriptor>>();
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return ServiceDescriptors.Values.SelectMany(item => item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
