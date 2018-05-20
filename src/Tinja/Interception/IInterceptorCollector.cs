using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptorCollector
    {
        IEnumerable<InterceptionTargetBinding> Collect(Type baseType, Type implementionType);
    }
}
