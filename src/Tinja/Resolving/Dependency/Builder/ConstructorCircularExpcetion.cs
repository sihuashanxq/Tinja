using System;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ConstructorCircularExpcetion : Exception
    {
        public Type ServiceType { get; }

        public ConstructorCircularExpcetion(Type serviceType, string message) : base(message)
        {
            ServiceType = serviceType;
        }
    }
}
