using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        private readonly IMethodInvocationInvokerBuilder _builder;

        public MethodInvocationExecutor(IMethodInvocationInvokerBuilder builder)
        {
            _builder = builder ?? throw new NullReferenceException(nameof(builder));
        }

        public virtual TResult Execute<TResult>(IMethodInvocation inv)
        {
            var task = ExecuteCore(inv);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            task.Wait();

            return (TResult)inv.Result;
        }

        public virtual ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation inv)
        {
            return new ValueTask<TResult>(ExecuteAsync<TResult>(inv));
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(IMethodInvocation inv)
        {
            var task = ExecuteCore(inv);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            var taskAwaiter = task.GetAwaiter();
            var taskCompletionSource = new TaskCompletionSource<TResult>();

            taskAwaiter.OnCompleted(() => taskCompletionSource.SetResult((TResult)inv.Result));

            return taskCompletionSource.Task;
        }

        protected Task ExecuteCore(IMethodInvocation invocation)
        {
            var invoker = _builder.Build(invocation.MethodInfo);
            if (invoker == null)
            {
                throw new NullReferenceException(nameof(invoker));
            }

            return invoker.InvokeAsync(invocation);
        }
    }
}
