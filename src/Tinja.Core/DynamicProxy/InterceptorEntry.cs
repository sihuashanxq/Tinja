using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy
{
    public class InterceptorEntry
    {
        public IInterceptor Interceptor { get; }

        public InterceptorMetadata Metadata { get; }

        public InterceptorEntry(IInterceptor interceptor, InterceptorMetadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));
        }
    }
}
