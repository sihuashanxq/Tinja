using System;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    /// <summary>
    /// a descriptor for Interceptor
    /// </summary>
    public class InterceptorDefinition
    {
        public long Order { get; }

        public MemberInfo Target { get; }

        public Type InterceptorType { get; }

        public InterceptorDefinition(long order, Type interceptorType, MemberInfo target)
        {
            if (interceptorType == null)
            {
                throw new NullReferenceException(nameof(interceptorType));
            }

            if (target == null)
            {
                throw new NullReferenceException(nameof(target));
            }

            if (typeof(IInterceptor).IsAssignableFrom(interceptorType))
            {
                throw new InvalidOperationException($"Type:{InterceptorType.FullName} must be an IInterceptor");
            }

            Order = order;
            Target = target;
            InterceptorType = interceptorType;
        }
    }
}
