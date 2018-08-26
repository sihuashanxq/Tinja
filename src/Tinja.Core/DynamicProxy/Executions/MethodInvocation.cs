using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocation : IMethodInvocation
    {
        public MethodInfo Method { get; }

        public MemberInfo Target { get; }

        public object[] Arguments { get; }

        public object ProxyInstance { get; }

        public object ResultValue { get; set; }

        public MethodInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] argumentValues, MemberInfo targetMember)
        {
            Method = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));
            Target = targetMember ?? throw new NullReferenceException(nameof(targetMember));
            Arguments = argumentValues ?? throw new NullReferenceException(nameof(argumentValues));
            ProxyInstance = instance ?? throw new NullReferenceException(nameof(instance));

            if (Method.IsGenericMethodDefinition)
            {
                if (genericArguments == null)
                {
                    throw new InvalidOperationException($"{Method.Name} MakeGenericMethod failed!");
                }

                Method = Method.MakeGenericMethod(genericArguments);
            }
        }
    }
}
