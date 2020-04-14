using System;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    /// <summary>
    /// a metadata for Interceptor
    /// </summary>
    public class InterceptorMetadata
    {
        public long? Order { get; private set; }

        public MemberInfo Target { get; private set; }

        public Type InterceptorType { get; private set; }

        public Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> Handler { get; private set; }

        public InterceptorMetadata(InterceptorAttribute attribute, MemberInfo target)
        {
            if (attribute is IInterceptor interceptor)
            {
                Initialize((m, next) => interceptor.InvokeAsync(m, next), target, attribute.Priority);
            }
            else
            {
                Initialize(attribute.InterceptorType, target, attribute.Priority);
            }
        }

        public InterceptorMetadata(Type interceptorType, MemberInfo target, long? order = null)
        {
            Initialize(interceptorType, target, order);
        }

        public InterceptorMetadata(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler, MemberInfo target, long? order = null)
        {
            Initialize(handler, target, order);
        }

        private void Initialize(Type interceptorType, MemberInfo target, long? order = null)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            if (interceptorType.IsNotType<IInterceptor>())
            {
                throw new ArgumentException($"Type:{InterceptorType.FullName} must be an IInterceptor");
            }

            Order = order;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            InterceptorType = interceptorType;
        }

        private void Initialize(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler, MemberInfo target, long? order = null)
        {
            Order = order;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
