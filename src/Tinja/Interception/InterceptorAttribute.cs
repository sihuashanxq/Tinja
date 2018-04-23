using System;

namespace Tinja.Interception
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class InterceptorAttribute : Attribute
    {
        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (!typeof(IIntereceptor).IsAssignableFrom(interceptorType))
            {
                throw new NotSupportedException($"type:{interceptorType.FullName} must implement the interface{typeof(IIntereceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }
    }
}
