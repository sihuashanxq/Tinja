using System;
using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorCollector
    {
        IEnumerable<InterceptorEntry> Collect(Type baseType, Type implementionType);
    }
}
