using System;

namespace Tinja.Resolving.Dependency
{
    public class CallCircularException : Exception
    {
        public Type ServiceType { get; }

        public CallCircularException(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
