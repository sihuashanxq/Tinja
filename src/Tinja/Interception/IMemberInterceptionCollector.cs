using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IMemberInterceptionCollector
    {
        IEnumerable<MemberInterception> Collect(Type serviceType, Type implementionType, bool caching = true);
    }
}
