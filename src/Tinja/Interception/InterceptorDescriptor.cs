using System;
using System.Reflection;
using Tinja.Extensions;

namespace Tinja.Interception
{
    /// <summary>
    /// a descriptor for Interceptor
    /// </summary>
    public class InterceptorDescriptor
    {
        public long Order { get; }

        public Type InterceptorType { get; }

        public MemberInfo TargetMember { get; }

        public InterceptorDescriptor(long order, Type interceptorType, MemberInfo targetMember)
        {
            if (interceptorType == null)
            {
                throw new NullReferenceException(nameof(interceptorType));
            }

            if (targetMember == null)
            {
                throw new NullReferenceException(nameof(targetMember));
            }

            if (interceptorType.IsNot<IInterceptor>())
            {
                throw new InvalidOperationException($"Type:{interceptorType.FullName} is not a type of IInterceptor");
            }

            Order = order;
            TargetMember = targetMember;
            InterceptorType = interceptorType;
        }
    }
}
