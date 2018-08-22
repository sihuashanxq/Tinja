using System;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    /// <summary>
    /// a metadata for Interceptor
    /// </summary>
    public class InterceptorMetadata
    {
        public long? RankOrder { get; }

        public MemberInfo Target { get; }

        public Type InterceptorType { get; }

        public Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> Handler { get; }

        public InterceptorMetadata(Type interceptorType, MemberInfo target, long? rankOrder = null)
        {
            if (interceptorType == null)
            {
                throw new NullReferenceException(nameof(interceptorType));
            }

            if (interceptorType.IsNotType(interceptorType))
            {
                throw new InvalidOperationException($"Type:{InterceptorType.FullName} must be an IInterceptor");
            }

            RankOrder = rankOrder;
            Target = target ?? throw new NullReferenceException(nameof(target));
            InterceptorType = interceptorType;
        }

        public InterceptorMetadata(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler, MemberInfo target, long? rankOrder = null)
        {
            Target = target ?? throw new NullReferenceException(nameof(target));
            Handler = handler ?? throw new NullReferenceException(nameof(handler));
            RankOrder = rankOrder;
        }
    }
}
