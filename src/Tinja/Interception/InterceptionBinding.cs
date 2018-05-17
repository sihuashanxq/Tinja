using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptionBinding
    {
        public IInterceptor Interceptor { get; }

        public InterceptionMetadata Metadata { get; }

        public InterceptionBinding(IInterceptor interceptor, InterceptionMetadata metadata)
        {
            Interceptor = interceptor;
            Metadata = metadata;
        }
    }
}
