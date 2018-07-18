using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Core.DynamicProxy
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class InterceptorAttribute : Attribute
    {
        public long Order { get; set; } = -1;

        public bool Inherited { get; set; } = false;

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (interceptorType.IsNot<IInterceptor>())
            {
                throw new NotSupportedException($"type:{interceptorType.FullName} must implement the interface{typeof(IInterceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }
    }
}
