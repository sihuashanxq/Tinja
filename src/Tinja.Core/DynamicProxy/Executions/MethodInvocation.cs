using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocation : IMethodInvocation
    {
        public MethodInfo Method { get; }

        public object[] Parameters { get; }

        public object ProxyInstance { get; }

        public MemberInfo TargetMember { get; }

        public object ResultValue { get; set; }

        public MethodInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] argumentValues, MemberInfo targetMember)
        {
            ProxyInstance = instance ?? throw new NullReferenceException(nameof(instance));
            Method = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));
            Parameters = argumentValues ?? throw new NullReferenceException(nameof(argumentValues));
            TargetMember = targetMember ?? throw new NullReferenceException(nameof(targetMember));

            if (Method.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidOperationException("MakeGenericMethod failed!");
                }

                Method = Method.MakeGenericMethod(genericArguments);
            }
        }
    }
}
