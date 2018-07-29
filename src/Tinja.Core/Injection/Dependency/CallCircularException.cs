using System;

namespace Tinja.Core.Injection.Dependency
{
    /// <summary>
    /// the exception when circular dependency 
    /// </summary>
    public class CallCircularException : Exception
    {
        public Type ServiceType { get; }

        public CallCircularException(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
