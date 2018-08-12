using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        public virtual TResult Execute<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            var value = ExecuteMethodInvocation(invoker, invocation);
            if (value is Task<TResult> tTask)
            {
                return tTask.Result;
            }

            if (value is Task<object> oTask)
            {
                return (TResult)oTask.Result;
            }

            if (value is TResult tResult)
            {
                return tResult;
            }

            throw new InvalidCastException($"Method:{invocation.Method}return value must be a Task<T> or T");
        }

        public virtual Task ExecuteVoidAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            var value = ExecuteMethodInvocation(invoker, invocation);
            if (value is Task task)
            {
                return task;
            }

            throw new InvalidCastException($"Method:{invocation.Method}return value must be a Task");
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            var value = ExecuteMethodInvocation(invoker, invocation);
            if (value is Task<TResult> task)
            {
                return task;
            }

            if (value is TResult tResult)
            {
                return Task.FromResult(tResult);
            }

            throw new InvalidCastException($"Method:{invocation.Method}return value must be a Task<T> or T");
        }

        public virtual ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            return new ValueTask<TResult>(ExecuteAsync<TResult>(invoker, invocation));
        }

        protected object ExecuteMethodInvocation(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            if (invoker == null)
            {
                throw new NullReferenceException(nameof(invoker));
            }

            var task = invoker.InvokeAsync(invocation);
            if (task == null)
            {
                throw new NullReferenceException(nameof(task));
            }

            if (task.Exception != null)
            {
                throw task.Exception;
            }

            return invocation.ResultValue;
        }
    }
}
