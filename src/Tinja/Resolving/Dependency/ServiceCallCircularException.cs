using System;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallCircularException : Exception
    {
        public Type ServiceType { get; }

        public ServiceCallCircularException(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
