using System;
using Tinja.Abstractions.DynamicProxy.Definitions;

namespace Tinja.Abstractions.DynamicProxy
{
    public class InterceptorEntry
    {
        public IInterceptor Interceptor { get; }

        public InterceptorDefinition Definition { get; }

        public InterceptorEntry(IInterceptor interceptor, InterceptorDefinition definition)
        {
            Definition = definition ?? throw new NullReferenceException(nameof(definition));
            Interceptor = interceptor ?? throw new NullReferenceException(nameof(interceptor));
        }
    }
}
