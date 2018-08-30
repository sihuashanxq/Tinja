using System;

namespace Tinja.Core.Injection.Dependencies
{
    /// <inheritdoc />
    public class CallCircularException : Exception
    {
        public Type ServiceType { get; }

        public CallDependElementScope CallScope { get; }

        public CallCircularException(Type serviceType, CallDependElementScope callScope, string message) : base(message)
        {
            CallScope = callScope;
            ServiceType = serviceType;
        }
    }
}
