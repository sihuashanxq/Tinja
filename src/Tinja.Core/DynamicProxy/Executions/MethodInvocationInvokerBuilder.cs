using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocationInvokerBuilder : IMethodInvocationInvokerBuilder
    {
        private readonly IObjectMethodExecutorProvider _methodExecutorProvider;

        private readonly Dictionary<MethodInfo, IMethodInvocationInvoker> _invokers;

        public MethodInvocationInvokerBuilder(IObjectMethodExecutorProvider methodExecutorProvider)
        {
            _methodExecutorProvider = methodExecutorProvider;
            _invokers = new Dictionary<MethodInfo, IMethodInvocationInvoker>();
        }

        public IMethodInvocationInvoker Build(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            if (_invokers.TryGetValue(methodInfo, out var invoker))
            {
                return invoker;
            }

            return _invokers[methodInfo] = BuildMethodInvocationInvoker(methodInfo);
        }

        protected virtual IMethodInvocationInvoker BuildMethodInvocationInvoker(MethodInfo methodInfo)
        {
            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
            }

            return new MethodInvocationInvoker(invocation =>
            {
                var stack = CreateMethodExecuteStack(executor);
                if (!stack.Any())
                {
                    throw new InvalidOperationException("build invoker failed!");
                }

                if (invocation.Interceptors == null ||
                    invocation.Interceptors.Length == 0)
                {
                    return stack.Pop()(invocation);
                }

                for (var i = invocation.Interceptors.Length - 1; i >= 0; i--)
                {
                    var next = stack.Pop();
                    var item = invocation.Interceptors[i];

                    stack.Push(async inv => await item.InvokeAsync(inv, next));
                }

                return stack.Pop()(invocation);
            });
        }

        private static Stack<Func<IMethodInvocation, Task>> CreateMethodExecuteStack(IObjectMethodExecutor executor)
        {
            var stack = new Stack<Func<IMethodInvocation, Task>>();

            stack.Push(async inv =>
            {
                if (inv.MethodInfo.IsAbstract || inv.MethodInfo.DeclaringType.IsInterface)
                {
                    await Task.CompletedTask;
                    return;
                }

                inv.SetResultValue(await executor.ExecuteAsync(inv.Instance, inv.ArgumentValues));
            });

            return stack;
        }
    }
}
