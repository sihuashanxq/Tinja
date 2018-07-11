using System;

namespace Tinja.Interception.Refactor
{
    public class InterceptorEntry
    {
        public IInterceptor Interceptor { get; }

        public InterceptorDescriptor Descriptor { get; }

        public InterceptorEntry(IInterceptor interceptor, InterceptorDescriptor descriptor)
        {
            Interceptor = interceptor ?? throw new NullReferenceException(nameof(interceptor));
            Descriptor = descriptor ?? throw new NullReferenceException(nameof(descriptor));
        }
    }
}
