using System;
using System.Collections.Generic;

namespace Tinja.Resolving.Context
{
    public class ServiceManyContext : ServiceContext
    {
        public List<ServiceContext> Elements { get; set; }

        public Type CollectionType { get; set; }
    }
}
