using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptorCollector
    {
        IEnumerable<MemberInterceptionBinding> Collect(Type baseType, Type implementionType);
    }
}
