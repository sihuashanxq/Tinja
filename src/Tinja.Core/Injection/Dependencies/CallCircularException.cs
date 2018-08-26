using System;

namespace Tinja.Core.Injection.Dependencies
{
    /// <inheritdoc />
    public class CallCircularException : Exception
    {
        public Type ServiceType { get; }

        public CallCircularException(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
