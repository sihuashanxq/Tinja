using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Tinja.Interception
{
    public interface IMethodInvocation
    {
        object Target { get; }

        object ReturnValue { get; set; }

        MethodInfo TargetMethod { get; }

        object[] ParameterValues { get; }

        IInterceptor[] Interceptors { get; }
    }

    public class MethodInvocation : IMethodInvocation
    {
        public object Target { get; }

        public MethodInfo TargetMethod { get; }

        public object ReturnValue { get; set; }

        public object[] ParameterValues { get; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object target, MethodInfo targetMethod, object[] parameterValues, IInterceptor[] interceptors)
        {
            Target = target;
            TargetMethod = targetMethod;
            ParameterValues = parameterValues;
            Interceptors = interceptors;
        }
    }

    public class PropertyMethodInvocation : MethodInvocation
    {
        public PropertyInfo TargetProperty { get; }

        public PropertyMethodInvocation(object target, MethodInfo targetMethod, object[] parameterValues, IInterceptor[] interceptors, PropertyInfo propertyInfo) 
            : base(target, targetMethod, parameterValues, interceptors)
        {
            TargetProperty = propertyInfo;
        }
    }
}
