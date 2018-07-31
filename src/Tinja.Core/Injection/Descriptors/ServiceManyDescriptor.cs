using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceManyDescriptor : ServiceDescriptor
    {
        public Type ElementType { get; set; }

        public List<ServiceDescriptor> Elements { get; set; }
    }
}
