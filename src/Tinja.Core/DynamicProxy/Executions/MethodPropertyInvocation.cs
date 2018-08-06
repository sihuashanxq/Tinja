using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodPropertyInvocation : MethodInvocation
    {
        public PropertyInfo Property { get; }

        public override MethodInvocationType InvocationType => MethodInvocationType.Property;

        public MethodPropertyInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] argumentValues, IInterceptor[] interceptors, PropertyInfo propertyInfo)
            : base(instance, methodInfo, genericArguments, argumentValues, interceptors)
        {
            Property = propertyInfo ?? throw new NullReferenceException(nameof(propertyInfo));
        }
    }
}
