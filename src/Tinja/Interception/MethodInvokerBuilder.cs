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
            return Cache.GetOrAdd(methodInfo, (m) => Build(ObjectMethodExecutorProvider.GetExecutor(m)));
        }

        protected virtual Func<IMethodInvocation, Task> Build(IObjectMethodExecutor executor)
        {
            return inv =>
            {
                var interceptors = inv.Interceptors;
                var callStack = new Stack<Func<IMethodInvocation, Task>>();

                callStack.Push(async (invocation) =>
                {
                    inv.ReturnValue = await executor.ExecuteAsync(invocation.Target, invocation.ParameterValues);
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
