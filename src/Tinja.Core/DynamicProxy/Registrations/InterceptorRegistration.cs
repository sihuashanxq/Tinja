using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Registrations
{
    public class InterceptorRegistration : IInterceptorRegistration
    {
        internal long? Order { get; set; }

        internal Func<MemberInfo, bool> TargetFilter { get; set; }

        public void SetOrder(long rankOrder)
        {
            Order = rankOrder;
        }

        public void SetFilter(Func<MemberInfo, bool> targetFilter)
        {
            TargetFilter = targetFilter ?? throw new NullReferenceException(nameof(targetFilter));
        }
    }
}
