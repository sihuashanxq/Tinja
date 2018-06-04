using System;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallCircularExpcetion : Exception
    {
        public Type ServiceType { get; }

        public ServiceCallCircularExpcetion(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
