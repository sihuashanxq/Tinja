using System;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        protected IMethodInvokerBuilder Builder { get; }

        public MethodInvocationExecutor(IMethodInvokerBuilder builder)
        {
            Builder = builder;
        }

        public object Execute(MethodInvocation invocation)
        {
            var invoker = Builder.Build(invocation.TargetMethod);
            if (invoker == null)
            {
                throw new NullReferenceException(nameof(invoker));
            }

            var task = invoker(invocation);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            if (invocation.TargetMethod.ReturnType.IsTask())
            {
                var tcs = new TaskCompletionSource<object>();
                var awaiter = task.GetAwaiter();

                awaiter.OnCompleted(() => tcs.SetResult(invocation.ReturnValue));

                return tcs.Task;
            }

            task.Wait();
            return invocation.ReturnValue;
        }
    }
}
