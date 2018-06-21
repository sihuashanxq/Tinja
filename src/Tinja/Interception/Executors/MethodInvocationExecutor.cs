using System;
using System.Threading.Tasks;
using Tinja.Extensions;

namespace Tinja.Interception.Executors
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
            return (TResult)inv.ResultValue;
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
                taskCompletionSource.SetResult((TResult)inv.ResultValue);
            });

            return taskCompletionSource.Task;
        }


        protected Task ExecuteCore(IMethodInvocation inv)
        {
            var invoker = Builder.Build(inv.TargetMethod);
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
