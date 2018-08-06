using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocation : IMethodInvocation
    {
        public object Instance { get; }

        public MethodInfo MethodInfo { get; }

        public object[] ArgumentValues { get; }

        public IInterceptor[] Interceptors { get; }

        public object Result { get; protected set; }

        public virtual MethodInvocationType InvocationType => MethodInvocationType.Method;

        public MethodInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] argumentValues, IInterceptor[] interceptors)
        {
            Instance = instance ?? throw new NullReferenceException(nameof(instance));
            MethodInfo = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));
            Interceptors = interceptors;
            ArgumentValues = argumentValues;

            if (MethodInfo.IsGenericMethod)
            {
                if (genericArguments == null)
                {
                    throw new InvalidOperationException("MakeGenericMethod failed!");
                }

                MethodInfo = MethodInfo.MakeGenericMethod(genericArguments);
            }
        }

        public virtual bool SetResultValue(object value)
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
