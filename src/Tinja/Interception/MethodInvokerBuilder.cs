using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        private IEnumerable<IInterceptorSelector> _interceptorSelectors;

        private IObjectMethodExecutorProvider _objectMethodExecutorProvider;

        private ConcurrentDictionary<MethodInfo, Func<MethodInvocation, object>> _invokers;

        public MethodInvokerBuilder(IObjectMethodExecutorProvider objectMethodExecutorProvider)
        {
            _invokers = new ConcurrentDictionary<MethodInfo, Func<MethodInvocation, object>>();
            _objectMethodExecutorProvider = objectMethodExecutorProvider;
        }

        public Func<MethodInvocation, object> Build(MethodInvocation methodInvocation)
        {
            return null;
        }

        protected virtual Func<MethodInvocation, object> Build(MethodInfo methodInfo)
        {
            var executor = _objectMethodExecutorProvider.GetExecutor(methodInfo);

            return (invocation) =>
           {
               var interceptors = invocation.Proxy.GetInterceptors();
               if (interceptors == null)
               {
                   return executor.ExecuteAsync(invocation.Target, invocation.ParameterValues);
               }

               foreach (var item in _interceptorSelectors)
               {
                   interceptors = item.Select(invocation.Target, methodInfo, interceptors);
               }

               for (var i = interceptors.Length - 1; i >= 0; i--)
               {

               }
           };
        }
    }
}
