using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptorCollector
    {
        IEnumerable<InterceptorEntry> Collect(Type baseType, Type implementionType);
    }
}
