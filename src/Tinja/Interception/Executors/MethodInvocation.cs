using System;
using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodInvocation : IMethodInvocation
    {
        public object Object { get; }

        public MethodInfo Method { get; }

        public object ResultValue { get; set; }

        public object[] ParameterValues { get; }

        public Type TargetType { get; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object proxy, Type proxyTargetType, MethodInfo targetMethod, Type[] genericArguments, object[] parameterValues, IInterceptor[] interceptors)
        {
            Object = proxy;
            TargetType = proxyTargetType;
            ParameterValues = parameterValues;
            Interceptors = interceptors;
            Method = targetMethod;

            if (Method.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidCastException("MakeGenericMethod Faild!");
                }

                Method = Method.MakeGenericMethod(genericArguments);
            }
        }
    }
}
