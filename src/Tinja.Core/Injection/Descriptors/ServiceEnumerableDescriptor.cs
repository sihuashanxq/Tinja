using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceEnumerableDescriptor : ServiceDescriptor
    {
        public Type ElementType { get; set; }

        public List<ServiceDescriptor> Elements { get; set; }
    }
}
