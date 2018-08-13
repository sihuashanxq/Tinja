using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        public TResult Execute<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            invoker.InvokeAsync(invocation).GetAwaiter().GetResult();

            return GetResult<TResult>(invocation);
        }

        public async Task ExecuteVoidAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            await GetResultAsync(invocation);
        }

        public async Task<TResult> ExecuteAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await invoker.InvokeAsync(invocation);

            return await GetResultAsync<TResult>(invocation);
        }

        public async ValueTask ExecuteValueTaskVoidAsync(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            await ExecuteVoidAsync(invoker, invocation);
        }

        public async ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocationInvoker invoker, IMethodInvocation invocation)
        {
            return await ExecuteAsync<TResult>(invoker, invocation);
        }

        private static TResult GetResult<TResult>(IMethodInvocation invocation)
        {
            if (invocation.ResultValue is Task<TResult> taskResult)
            {
                return taskResult.Result;
            }

            if (invocation.ResultValue is TResult tResult)
            {
                return tResult;
            }

            if (invocation.ResultValue is Task task)
            {
                task.Wait();
                return default(TResult);
            }

            throw new InvalidCastException($"Method:{invocation.Method}return value must be a Task<T> or T");
        }

        private static async Task GetResultAsync(IMethodInvocation invocation)
        {
            if (invocation.ResultValue is Task task)
            {
                await task;
            }
        }

        private static async Task<TResult> GetResultAsync<TResult>(IMethodInvocation invocation)
        {
            if (invocation.ResultValue is Task<TResult> taskResult)
            {
                return await taskResult;
            }

            if (invocation.ResultValue is TResult tResult)
            {
                return tResult;
            }

            if (invocation.ResultValue is Task task)
            {
                await task;
                return default(TResult);
            }

            throw new InvalidCastException($"Method:{invocation.Method}return value must be a Task<T> or T");
        }
    }
}
