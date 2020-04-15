using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceEnumerableEntry : ServiceEntry
    {
        public Type ElementType { get; set; }

        public List<ServiceEntry> Elements { get; set; }
    }
}
