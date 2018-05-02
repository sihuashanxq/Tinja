using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Tinja.Interception
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        private ConcurrentDictionary<MethodInfo, Func<object, object[], object>> _invokers;

        public MethodInvokerBuilder()
        {
            _invokers = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
        }

        public Func<object, object[], object> Build(MethodInvocation methodInvocation)
        {
            return null;
        }

        //private Func<object, object[], object> BuildDelegate(MethodInvocation methodInvocation)
        //{
        //    for (var i = 0; i < methodInvocation.Intereceptors.Length; i++)
        //    {

        //    }
        //}
    }
}
