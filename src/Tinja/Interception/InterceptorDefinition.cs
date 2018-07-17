using System;
using System.Reflection;
using Tinja.Extensions;

namespace Tinja.Interception
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

            if (interceptorType.IsNot<IInterceptor>())
            {
                throw new InvalidOperationException($"Type:{interceptorType.FullName} is not a type of IInterceptor");
            }

            Order = order;
            Target = target;
            InterceptorType = interceptorType;
        }
    }
}
