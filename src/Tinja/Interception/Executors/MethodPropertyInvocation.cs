﻿using System;
using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodPropertyInvocation : MethodInvocation
    {
        public PropertyInfo TargetProperty { get; }

        public MethodPropertyInvocation(object target, Type proxyTargetType, MethodInfo targetMethod, Type[] genericArguments, object[] parameterValues, IInterceptor[] interceptors, PropertyInfo propertyInfo)
            : base(target, proxyTargetType, targetMethod, genericArguments, parameterValues, interceptors)
        {
            TargetProperty = propertyInfo;
        }
    }
}
