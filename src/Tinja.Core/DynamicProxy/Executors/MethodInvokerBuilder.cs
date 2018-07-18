using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executors;

namespace Tinja.Core.DynamicProxy.Executors
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        private readonly IObjectMethodExecutorProvider _methodExecutorProvider;

        private readonly Dictionary<MethodInfo, Func<IMethodInvocation, Task>> _cache;

        public MethodInvokerBuilder(IObjectMethodExecutorProvider methodExecutorProvider)
        {
            _methodExecutorProvider = methodExecutorProvider;
            _cache = new Dictionary<MethodInfo, Func<IMethodInvocation, Task>>();
        }

        public Func<IMethodInvocation, Task> Build(MethodInfo methodInfo)
        {
            if (_cache.TryGetValue(methodInfo, out var executor))
            {
                return executor;
            }

            return _cache[methodInfo] = BuildInvoker(methodInfo);
        }

        protected virtual Func<IMethodInvocation, Task> BuildInvoker(MethodInfo methodInfo)
        {
            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
            }

            return invocation =>
            {
                var stack = CreateExecuteStack(executor);
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
            };
        }

        private static Stack<Func<IMethodInvocation, Task>> CreateExecuteStack(IObjectMethodExecutor executor)
        {
            var stack = new Stack<Func<IMethodInvocation, Task>>();

            stack.Push(async inv =>
            {
                if (inv.MethodInfo.IsAbstract || inv.MethodInfo.DeclaringType.IsInterface)
                {
                    await Task.CompletedTask;
                    return;
                }

                inv.SetResultValue(await executor.ExecuteAsync(inv.Instance, inv.Arguments));
            });

            return stack;
        }
    }
}
