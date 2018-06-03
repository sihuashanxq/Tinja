using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class MemberInterception
    {
        public Type Interceptor { get; set; }

        public Dictionary<MemberInfo, long> Prioritys { get; set; }
    }
}
