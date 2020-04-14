using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Extensions
{
    public static class InterceptorRegistrationExtensions
    {
        public static IInterceptorRegistration Order(this IInterceptorRegistration registration, long order)
        {
            registration.SetPriority(order);
            return registration;
        }

        public static IInterceptorRegistration When(this IInterceptorRegistration registration,Func<MemberInfo,bool> filter)
        {
            registration.SetFilter(filter);
            return registration;
        }
    }
}
