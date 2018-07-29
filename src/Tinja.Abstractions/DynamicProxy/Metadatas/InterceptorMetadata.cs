using System;
using System.Reflection;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    /// <summary>
    /// a metadata for Interceptor
    /// </summary>
    public class InterceptorMetadata
    {
        public long Order { get; }

        public MemberInfo Target { get; }

        public Type InterceptorType { get; }

        public InterceptorMetadata(long order, Type interceptorType, MemberInfo target)
        {
            if (interceptorType == null)
            {
                throw new NullReferenceException(nameof(interceptorType));
            }

            if (interceptorType.IsNotType(interceptorType))
            {
                throw new InvalidOperationException($"Type:{InterceptorType.FullName} must be an IInterceptor");
            }

            Order = order;
            Target = target ?? throw new NullReferenceException(nameof(target));
            InterceptorType = interceptorType;
        }
    }
}
