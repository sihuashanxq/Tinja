using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class MemberInterception
    {
        public Type InterceptorType { get; set; }

        public Dictionary<MemberInfo, long> MemberOrders { get; set; }
    }
}
