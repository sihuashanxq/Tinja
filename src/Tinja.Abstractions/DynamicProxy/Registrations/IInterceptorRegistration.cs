using System;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Registrations
{
    public interface IInterceptorRegistration
    {
        void SetFilter(Func<MemberInfo, bool> filter);

        void SetPriority(long order);
    }
}
