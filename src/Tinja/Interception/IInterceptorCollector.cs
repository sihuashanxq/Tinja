using System;

namespace Tinja.Interception
{
    public interface IInterceptorCollector
    {
        InterceptionBinding[] Collect(Type serviceType, Type implementionType);
    }
}
