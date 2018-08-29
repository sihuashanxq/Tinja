using System;
using System.Reflection;

namespace Tinja.Core.Injection.Dependencies
{
    public class ServicePropertyRequiredException : Exception
    {
        public Type Type { get; }

        public PropertyInfo PropertyInfo { get; }

        public ServicePropertyRequiredException(Type type, PropertyInfo propertyInfo)
        {
            Type = type;
            PropertyInfo = propertyInfo;
        }
    }
}
