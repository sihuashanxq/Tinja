using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executors;

namespace Tinja.Core.DynamicProxy.Executors
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        protected IMethodInvokerBuilder Builder { get; }

        public MethodInvocationExecutor(IMethodInvokerBuilder builder)
        {
            Builder = builder;
        }

        public virtual TResult Execute<TResult>(IMethodInvocation inv)
        {
            ExecuteCore(inv).Wait();

            return (TResult)inv.Result;
        }

        public virtual ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation inv)
        {
            return new ValueTask<TResult>(ExecuteAsync<TResult>(inv));
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(IMethodInvocation inv)
        {
            var task = ExecuteCore(inv);
            var taskCompletionSource = new TaskCompletionSource<TResult>();

            task.GetAwaiter().OnCompleted(() =>
            {
                taskCompletionSource.SetResult((TResult)inv.Result);
            });

            return taskCompletionSource.Task;
        }

        protected Task ExecuteCore(IMethodInvocation inv)
        {
            var invoker = Builder.Build(inv.MethodInfo);
            if (invoker == null)
            {
                throw new NullReferenceException(nameof(invoker));
            }

            var task = invoker(inv);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            return task;
        }
    }
}
