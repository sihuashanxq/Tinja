using System;

namespace Tinja.Interception
{
    public interface IInterceptorDescriptorCollector
    {
        InterceptorDescriptorCollection Collect(Type serviceType, Type implementionType);
    }
}
