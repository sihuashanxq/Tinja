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
            ExecuteCore(inv);
            return ((Task<TResult>)inv.Result).Result;
        }

        public virtual Task ExecuteVoidAsync(IMethodInvocation inv)
        {
            ExecuteCore(inv);
            return inv.Result as Task;
        }

        public virtual ValueTask<TResult> ExecuteValueTaskAsync<TResult>(IMethodInvocation inv)
        {
            return new ValueTask<TResult>(ExecuteAsync<TResult>(inv));
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(IMethodInvocation inv)
        {
            ExecuteCore(inv);
            return (Task<TResult>)inv.Result;
        }

        protected void ExecuteCore(IMethodInvocation invocation)
        {
            var invoker = _builder.Build(invocation);
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
        }
    }
}
