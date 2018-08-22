using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Registrations
{
    public class InterceptorRegistration : IInterceptorRegistration
    {
        internal long? RankOrder { get; set; }

        internal Func<MemberInfo, bool> TargetFilter { get; set; }

        public void SetRankOrder(long rankOrder)
        {
            RankOrder = rankOrder;
        }

        public void SetTargetFilter(Func<MemberInfo, bool> targetFilter)
        {
            TargetFilter = targetFilter ?? throw new NullReferenceException(nameof(targetFilter));
        }
    }
}
