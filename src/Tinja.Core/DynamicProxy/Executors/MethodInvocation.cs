using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Core.DynamicProxy.Executors
{
    public class MethodInvocation : IMethodInvocation
    {
        public object Instance { get; }

        public object[] Arguments { get; }

        public MethodInfo MethodInfo { get; }

        public object Result { get; protected set; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors)
        {
            Instance = instance;
            Arguments = arguments;
            MethodInfo = methodInfo;
            Interceptors = interceptors;

            if (MethodInfo.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidCastException("MakeGenericMethod Faild!");
                }

                MethodInfo = MethodInfo.MakeGenericMethod(genericArguments);
            }
        }

        public bool SetResultValue(object value)
        {
            if (MethodInfo.IsVoidMethod())
            {
                return false;
            }

            Result = value;
            return true;
        }
    }
}
