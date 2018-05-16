using System;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IInterceptor
    {
        Task InvokeAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next);
    }
}
