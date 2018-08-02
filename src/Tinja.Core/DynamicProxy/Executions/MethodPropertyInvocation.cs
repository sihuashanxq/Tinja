using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodPropertyInvocation : MethodInvocation
    {
        public PropertyInfo Property { get; }

        public MethodPropertyInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors, PropertyInfo propertyInfo)
            : base(instance, methodInfo, genericArguments, arguments, interceptors)
        {
            Property = propertyInfo;
        }
    }
}
