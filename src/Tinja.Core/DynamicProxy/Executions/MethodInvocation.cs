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

        public MethodInvocation(object proxyInstance, MethodInfo methodInfo, Type[] genericArguments, object[] arguments, MemberInfo target)
        {
            Method = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));
            Target = target ?? throw new NullReferenceException(nameof(target));
            Arguments = arguments ?? throw new NullReferenceException(nameof(arguments));
            ProxyInstance = proxyInstance ?? throw new NullReferenceException(nameof(proxyInstance));

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
