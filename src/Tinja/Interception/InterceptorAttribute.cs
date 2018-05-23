using System;

namespace Tinja.Interception
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = true, Inherited = true)]
    public class InterceptorAttribute : Attribute
    {
        public int Priority { get; set; } = -1;

        public bool Inherited { get; set; }

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (!typeof(IInterceptor).IsAssignableFrom(interceptorType))
            {
                throw new NotSupportedException($"type:{interceptorType.FullName} must implement the interface{typeof(IInterceptor).FullName}");
            }

            Inherited = true;
            InterceptorType = interceptorType;
        }
    }
}
