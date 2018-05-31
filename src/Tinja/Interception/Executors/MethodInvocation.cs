using System;
using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodInvocation : IMethodInvocation
    {
        public object Target { get; }

        public MethodInfo TargetMethod { get; }

        public object ResultValue { get; set; }

        public object[] ParameterValues { get; }

        public Type ProxyTargetType { get; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object proxy, Type proxyTargetType, MethodInfo targetMethod, Type[] genericArguments, object[] parameterValues, IInterceptor[] interceptors)
        {
            Target = proxy;
            ProxyTargetType = proxyTargetType;
            ParameterValues = parameterValues;
            Interceptors = interceptors;
            TargetMethod = targetMethod;

            if (TargetMethod.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidCastException("MakeGenericMethod Faild!");
                }

                TargetMethod = TargetMethod.MakeGenericMethod(genericArguments);
            }
        }
    }
}
