using System;

namespace Tinja.Interception
{
    public class InterceptorEntry
    {
        public IInterceptor Interceptor { get; }

        public InterceptorDefinition Descriptor { get; }

        public InterceptorEntry(IInterceptor interceptor, InterceptorDefinition descriptor)
        {
            Interceptor = interceptor ?? throw new NullReferenceException(nameof(interceptor));
            Descriptor = descriptor ?? throw new NullReferenceException(nameof(descriptor));
        }
    }
}
