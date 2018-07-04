using System;
using System.Reflection;

namespace Tinja.Resolving.Context
{
    public class ServiceConstrcutorContext : ServiceContext
    {
        public Type ImplementionType { get; set; }

        public ConstructorInfo[] Constrcutors => ImplementionType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
