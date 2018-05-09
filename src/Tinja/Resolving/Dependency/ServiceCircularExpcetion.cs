using System;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCircularExpcetion : Exception
    {
        public Type ServiceType { get; }

        public ServiceCircularExpcetion(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
