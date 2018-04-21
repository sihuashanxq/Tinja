using System;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ServiceConstructorCircularExpcetion : Exception
    {
        public Type ServiceType { get; }

        public ServiceConstructorCircularExpcetion(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
