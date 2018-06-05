using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        protected IEnumerable<IInterceptorSelector> InterceptorSelectors { get; }

        protected IObjectMethodExecutorProvider ObjectMethodExecutorProvider { get; }

        protected ConcurrentDictionary<MethodInfo, Func<IMethodInvocation, Task>> Cache { get; }

        public MethodInvokerBuilder(IEnumerable<IInterceptorSelector> interceptorSelectors, IObjectMethodExecutorProvider objectMethodExecutorProvider)
        {
            InterceptorSelectors = interceptorSelectors;
            ObjectMethodExecutorProvider = objectMethodExecutorProvider;
            Cache = new ConcurrentDictionary<MethodInfo, Func<IMethodInvocation, Task>>();
        }

        public Func<IMethodInvocation, Task> Build(MethodInfo methodInfo)
        {
            return Cache.GetOrAdd(methodInfo, BuildExecuteDelegate);
        }

        protected virtual Func<IMethodInvocation, Task> BuildExecuteDelegate(MethodInfo methodInfo)
        {
            return inv =>
            {
                var callStack = new Stack<Func<IMethodInvocation, Task>>();
                var interceptors = inv.Interceptors;

                callStack.Push(async invocation =>
                {
                    inv.ResultValue = await
                        ObjectMethodExecutorProvider
                            .GetExecutor(invocation.TargetMethod)
                            .ExecuteAsync(invocation.Target, invocation.ParameterValues);
                });

                foreach (var item in InterceptorSelectors)
                {
                    interceptors = item.Select(inv.Target, inv.TargetMethod, interceptors);
                }

                if (interceptors == null || interceptors.Length == 0)
                {
                    return callStack.Pop()(inv);
                }

                for (var i = interceptors.Length - 1; i >= 0; i--)
                {
                    var next = callStack.Pop();
                    var item = interceptors[i];

                    callStack.Push(async (invocation) =>
                    {
                        await item.InvokeAsync(invocation, next);
                    });
                }

                return callStack.Pop()(inv);
            };
        }
    }
}
