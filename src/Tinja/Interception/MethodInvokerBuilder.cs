using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public class MethodInvokerBuilder : IMethodInvokerBuilder
    {
        protected IObjectMethodExecutorProvider ObjectMethodExecutorProvider { get; }

        protected ConcurrentDictionary<MethodInfo, Func<IMethodInvocation, Task>> Cache { get; }

        public MethodInvokerBuilder(IObjectMethodExecutorProvider objectMethodExecutorProvider)
        {
            ObjectMethodExecutorProvider = objectMethodExecutorProvider;
            Cache = new ConcurrentDictionary<MethodInfo, Func<IMethodInvocation, Task>>();
        }

        public Func<IMethodInvocation, Task> Build(MethodInfo methodInfo)
        {
            return Cache.GetOrAdd(methodInfo, BuildExecuteDelegate);
        }

        protected virtual Func<IMethodInvocation, Task> BuildExecuteDelegate(MethodInfo methodInfo)
        {
            async Task ExecuteCore(IMethodInvocation invocation)
            {
                invocation.ResultValue = await ObjectMethodExecutorProvider
                    .GetExecutor(invocation.TargetMethod)
                    .ExecuteAsync(invocation.Target, invocation.ParameterValues);
            }

            Stack<Func<IMethodInvocation, Task>> CreateCallStack()
            {
                var stack = new Stack<Func<IMethodInvocation, Task>>();
                stack.Push(ExecuteCore);
                return stack;
            }

            return inv =>
            {
                var callStack = CreateCallStack();

                if (inv.Interceptors == null || inv.Interceptors.Length == 0)
                {
                    return callStack.Pop()(inv);
                }

                for (var i = inv.Interceptors.Length - 1; i >= 0; i--)
                {
                    var next = callStack.Pop();
                    var item = inv.Interceptors[i];

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
