using System;
using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Registrations
{
    public interface IInterceptorRegistration
    {
        void SetOrder(long order);

        void SetFilter(Func<MemberInfo, bool> filter);
    }
}
