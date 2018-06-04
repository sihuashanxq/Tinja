using System;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IInterceptor
    {
        Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next);
    }
}
