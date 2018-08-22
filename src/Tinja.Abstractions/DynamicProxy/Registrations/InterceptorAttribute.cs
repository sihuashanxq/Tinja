using System;
using System.Reflection;
using Tinja.Abstractions.Extensions;

namespace Tinja.Abstractions.DynamicProxy.Registrations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class InterceptorAttribute : Attribute, IInterceptorRegistration
    {
        public long? RankOrder { get; set; }

        public bool Inherited { get; set; }

        public Type InterceptorType { get; }

        public InterceptorAttribute(Type interceptorType)
        {
            if (interceptorType.IsNotType<IInterceptor>())
            {
                throw new NotSupportedException($"Type:{interceptorType.FullName} can not cast to the interface:{typeof(IInterceptor).FullName}");
            }

            InterceptorType = interceptorType;
        }

        public void SetRankOrder(long rankOrder)
        {
            //empty
        }

        public void SetTargetFilter(Func<MemberInfo, bool> matchPredicate)
        {
            //empty
        }
    }
}
