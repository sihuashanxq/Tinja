using System;
using System.Reflection;
using Tinja.Extensions;

namespace Tinja.Interception.Executors
{
    public class MethodInvocation : IMethodInvocation
    {
        public MethodInfo Method { get; }

        public object ReturnValue { get; protected set; }

        public object[] Arguments { get; }

        public object ContextObject { get; }

        public Type ProxyTargetType { get; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object contextObject, Type proxyTargetType, MethodInfo method, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors)
        {
            Method = method;
            Arguments = arguments;
            Interceptors = interceptors;
            ContextObject = contextObject;
            ProxyTargetType = proxyTargetType;

            if (Method.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidCastException("MakeGenericMethod Faild!");
                }

                Method = Method.MakeGenericMethod(genericArguments);
            }
        }

        public bool SetReturnValue(object value)
        {
            if (Method.IsVoidMethod())
            {
                return false;
            }

            ReturnValue = value;
            return true;
        }
    }
}
