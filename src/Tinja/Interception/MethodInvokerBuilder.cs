using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        private IEnumerable<IInterceptorSelector> _interceptorSelectors;

        private IObjectMethodExecutorProvider _objectMethodExecutorProvider;

        private ConcurrentDictionary<MethodInfo, Func<MethodInvocation, Task>> _invokers;

        public MethodInvokerBuilder(
            IEnumerable<IInterceptorSelector> interceptorSelectors,
            IObjectMethodExecutorProvider objectMethodExecutorProvider
        )
        {
            _interceptorSelectors = interceptorSelectors;
            _objectMethodExecutorProvider = objectMethodExecutorProvider;
            _invokers = new ConcurrentDictionary<MethodInfo, Func<MethodInvocation, Task>>();
        }

        public Func<MethodInvocation, Task> Build(MethodInfo methodInfo)
        {
            return _invokers.GetOrAdd(methodInfo, (m) => Build(_objectMethodExecutorProvider.GetExecutor(m)));
        }

        protected virtual Func<MethodInvocation, Task> Build(IObjectMethodExecutor executor)
        {
            return (invocation) =>
            {
                var interceptors = invocation.Interceptors;
                var stack = new Stack<Func<MethodInvocation, Task>>();

                stack.Push(async (inv) =>
                {
                    inv.ReturnValue = await executor.ExecuteAsync(invocation.Target, invocation.ParameterValues);
                });

                foreach (var item in _interceptorSelectors)
                {
                    interceptors = item.Select(invocation.Target, invocation.TargetMethod, interceptors);
                }

                if (interceptors == null || interceptors.Length == 0)
                {
                    return stack.Pop()(invocation);
                }

                for (var i = interceptors.Length - 1; i >= 0; i--)
                {
                    var next = stack.Pop();
                    var item = interceptors[i];

                    stack.Push(async (inv) =>
                    {
                        await item.InvokeAsync(inv, next);
                    });
                }

                return stack.Pop()(invocation);
            };
        }
    }
}
