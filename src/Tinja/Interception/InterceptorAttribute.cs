using System;

namespace Tinja.Interception
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class InterceptorAttribute : Attribute
    {
        public int Order { get; set; } = -1;

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (!typeof(IInterceptor).IsAssignableFrom(interceptorType))
            {
                throw new NotSupportedException($"type:{interceptorType.FullName} must implement the interface{typeof(IInterceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }
    }
}
