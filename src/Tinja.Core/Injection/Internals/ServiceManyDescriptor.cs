using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Internals
{
    public class ServiceManyDescriptor : ServiceDescriptor
    {
        public List<ServiceDescriptor> Elements { get; set; }

        public Type CollectionType { get; set; }
    }
}
