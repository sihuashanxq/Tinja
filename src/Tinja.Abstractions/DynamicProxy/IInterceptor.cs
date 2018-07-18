using System;
using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptor
    {
        Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next);
    }
}
