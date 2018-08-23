using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Extensions
{
    public static class InterceptorRegistrationExtensions
    {
        public static IInterceptorRegistration WithOrder(this IInterceptorRegistration registration, long rankOrder)
        {
            registration.SetRankOrder(rankOrder);

            return registration;
        }

        public static IInterceptorRegistration When(this IInterceptorRegistration registration,Func<MemberInfo,bool> targetFilter)
        {
            registration.SetTargetFilter(targetFilter);

            return registration;
        }
    }
}
