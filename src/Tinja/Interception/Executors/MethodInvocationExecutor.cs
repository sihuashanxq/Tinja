using System;
using System.Runtime.CompilerServices;
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

        public object Execute(IMethodInvocation inv)
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

            if (!inv.TargetMethod.ReturnType.IsTask() &&
                !inv.TargetMethod.ReturnType.IsValueTask())
            {
                task.Wait();

                return inv.ResultValue;
            }

            return GetAsyncReturnValue(task, inv);
        }

        private static object GetAsyncReturnValue(Task task, IMethodInvocation inv)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();

            if (inv.TargetMethod.ReturnType.IsValueTask())
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    var t = task;
                    taskCompletionSource.SetResult((int)inv.ResultValue);
                });

                return new ValueTask<int>(taskCompletionSource.Task);
            }

            task.GetAwaiter().OnCompleted(() =>
            {
                var t = task;
                taskCompletionSource.SetResult((int)inv.ResultValue);
            });

            return taskCompletionSource.Task;
        }
    }
}
