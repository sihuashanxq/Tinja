using System;
using System.Reflection;

namespace Tinja.Interception.Refactor
{
    /// <summary>
    /// a descriptor for Interceptor
    /// </summary>
    public class InterceptorDescriptor
    {
        public long Order { get; set; }

        public Type InterceptorType { get; set; }

        public MemberInfo BindTarget { get; set; }
    }
}
