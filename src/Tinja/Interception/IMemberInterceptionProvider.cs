using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    internal interface IMemberInterceptionProvider
    {
        IEnumerable<MemberInterception> GetInterceptions(Type serviceType, Type implementionType);
    }
}
