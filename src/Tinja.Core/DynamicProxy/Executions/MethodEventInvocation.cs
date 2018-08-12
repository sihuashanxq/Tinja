﻿using System;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodEventInvocation : MethodInvocation
    {
        public EventInfo EventInfo { get; }

        public override MethodInvocationType InvocationType => MethodInvocationType.Event;

        public MethodEventInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors, EventInfo eventInfo)
            : base(instance, methodInfo, genericArguments, arguments, interceptors)
        {
            EventInfo = eventInfo ?? throw new NullReferenceException(nameof(eventInfo));
        }
    }
}