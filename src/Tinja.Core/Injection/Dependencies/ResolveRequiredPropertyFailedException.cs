using System;
using System.Reflection;

namespace Tinja.Core.Injection.Dependencies
{
    public class ResolveRequiredPropertyFailedException : Exception
    {
        public Type Type { get; }

        public PropertyInfo PropertyInfo { get; }

        public CallDependElementScope CallScope { get; }

        public ResolveRequiredPropertyFailedException(Type type, CallDependElementScope callScope, PropertyInfo propertyInfo)
        {
            Type = type;
            CallScope = callScope;
            PropertyInfo = propertyInfo;
        }
    }
}
