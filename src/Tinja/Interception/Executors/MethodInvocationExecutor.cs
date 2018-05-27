using System;
using System.Threading.Tasks;
using Tinja.Interception.Executors;

namespace Tinja.Interception
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

                return inv.ReturnValue;
            }

            return GetAsyncReturnValue(task, new TaskCompletionSource<object>(), inv);
        }

        private static object GetAsyncReturnValue(Task task, TaskCompletionSource<object> taskCompletion, IMethodInvocation inv)
        {
            task.GetAwaiter().OnCompleted(() =>
            {
                taskCompletion.SetResult(inv.ReturnValue);
            });

            if (inv.TargetMethod.ReturnType.IsValueTask())
            {
                return Activator.CreateInstance(typeof(ValueTask<>).MakeGenericType(inv.TargetMethod.ReturnType.GetGenericArguments()), taskCompletion.Task);
            }

            return taskCompletion.Task;
        }
    }
}
