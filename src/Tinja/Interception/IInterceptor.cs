using System;
using System.Threading.Tasks;
using Tinja.Interception.Executors;

namespace Tinja.Interception
{
    public interface IInterceptor
    {
        Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next);
    }
}
