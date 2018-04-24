using System;

namespace Tinja.Interception
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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
