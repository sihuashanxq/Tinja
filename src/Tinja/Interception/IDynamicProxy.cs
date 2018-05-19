using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IDynamicProxy
    {
        object Target { get; }

        List<InterceptionTargetBinding> GetInterceptors();
    }
}
