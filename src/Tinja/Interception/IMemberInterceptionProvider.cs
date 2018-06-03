using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IMemberInterceptionProvider
    {
        IEnumerable<MemberInterception> GetInterceptions(Type serviceType, Type implementionType);
    }
}
