using System;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Registrations
{
    public interface IInterceptorRegistration
    {
        void SetRankOrder(long rankOrder);

        void SetTargetFilter(Func<MemberInfo, bool> filter);
    }
}
