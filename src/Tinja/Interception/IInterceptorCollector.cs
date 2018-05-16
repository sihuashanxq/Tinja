using System;

namespace Tinja.Interception
{
    public interface IInterceptorCollector
    {
        InterceptorBinding[] Collect(Type serviceType, Type implementionType);
    }
}
