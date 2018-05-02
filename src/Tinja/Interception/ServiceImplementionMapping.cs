using System;
using System.Reflection;

namespace Tinja.Interception
{
    public class ServiceImplementionMapping
    {
        public Type ServiceType { get; set; }

        public MethodInfo[] ServiceMethods { get; set; }

        public PropertyInfo[] ServiceProperties { get; set; }

        public Type ImplementionType { get; set; }

        public MethodInfo[] ImplementionMethods { get; set; }

        public PropertyInfo[] ImplementionProperties { get; set; }
    }
}
