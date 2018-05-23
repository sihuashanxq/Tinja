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

        public object Execute(MethodInvocation methodInvocation)
        {
            var invoker = Builder.Build(methodInvocation.TargetMethod);
            if (invoker == null)
            {
                throw new NullReferenceException(nameof(invoker));
            }

            var task = invoker(methodInvocation);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            if (methodInvocation.TargetMethod.ReturnType.IsTask())
            {
                var tcs = new TaskCompletionSource<object>();
                var awaiter = task.GetAwaiter();

                awaiter.OnCompleted(() => tcs.SetResult(methodInvocation.ReturnValue));

                return tcs.Task;
            }

            task.Wait();

            return methodInvocation.ReturnValue;
        }
    }
}
